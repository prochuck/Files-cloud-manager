using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using System.IO;
using System.Security.Cryptography;
using FileInfo = Files_cloud_manager.Models.FileInfo;

namespace Files_cloud_manager.Server.Services
{
    public class FilesSynchronizationService : IFilesSynchronizationService
    {
        /// <summary>
        /// Переделаь под контейнер синхронизации?
        /// </summary>
        private Dictionary<string, FileInfo> _filesInfos;
        private FileInfoGroup? _fileInfoGroup;
        private IUnitOfWork _unitOfWork;
        private bool _isSyncStarted;
        private string _basePath;
        private HashAlgorithm _hashService;

        public FilesSynchronizationService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _basePath = configuration["basePath"];
        }

        public bool StartSynchronization(int userId, int folderId)
        {
            _fileInfoGroup = _unitOfWork.FileInfoGroupRepostiory.Get(
                    e => e.Id == folderId,
                    new string[] { nameof(_fileInfoGroup.Files), nameof(_fileInfoGroup.Owner) }
                ).FirstOrDefault();

            if (_fileInfoGroup is null || _fileInfoGroup.OwnerId != userId)
            {
                return false;
            }

            _filesInfos = _fileInfoGroup.Files.ToDictionary(e => e.RelativePath);
            return true;
        }
        public bool CreateOrUpdateFile(string filePath, Stream originalFileStream)
        {
            if (!_isSyncStarted)
            {
                return false;
            }

            string fullPath = GetFullPath(filePath);

            FileInfo fileInfo;

            if (!_filesInfos.ContainsKey(filePath))
            {
                fileInfo = new FileInfo()
                {
                    RelativePath = filePath,
                    FileInfoGroupId = _fileInfoGroup.Id
                };
            }
            else
            {
                fileInfo = _filesInfos[filePath];
            }

            // todo передалать перезапись файлов.
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            fileInfo.Hash = CreateFileFromStream(originalFileStream, fullPath);

            if (!_filesInfos.ContainsKey(filePath))
            {
                _filesInfos.Add(filePath, fileInfo);
                _unitOfWork.FileInfoRepository.Create(fileInfo);
            }
            else
            {
                _unitOfWork.FileInfoRepository.Update(fileInfo);
            }
            return true;
        }
        public bool DeleteFile(string filePath)
        {
            if (!_filesInfos.ContainsKey(filePath))
            {
                return false;
            }

            FileInfo fileInfo = _filesInfos[filePath];
            string fullPath = GetFullPath(filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            _filesInfos.Remove(filePath);
            _unitOfWork.FileInfoRepository.Delete(fileInfo);

            return true;
        }
        public Stream GetFile(string filePath)
        {
            if (!_filesInfos.ContainsKey(filePath))
            {
                return null;
            }

            string fullPath = GetFullPath(filePath);

            if (File.Exists(filePath))
            {
                return new FileStream(filePath, FileMode.Open);
            }
            return null;
        }
        public List<FileInfoDTO> GetFiles()
        {
            return _fileInfoGroup.Files.Select(e => AutoMapper)
        }

        private string GetFullPath(string relativePath)
        {
            return $"{_basePath}/{_fileInfoGroup.Owner.Login}/{_fileInfoGroup.Name}/{relativePath}";
        }
        private byte[] CreateFileFromStream(Stream originalFileStream, string path)
        {
            _hashService.Initialize();
            FileStream fileStream = new FileStream(path, FileMode.CreateNew);
            long bytesToCopyLeft = originalFileStream.Length;
            byte[] buffer = new byte[_hashService.InputBlockSize];
            while (bytesToCopyLeft != 0)
            {
                int toRead = buffer.Length < bytesToCopyLeft ? buffer.Length : (int)bytesToCopyLeft;
                int readed = originalFileStream.Read(buffer, 0, toRead);

                if (readed == buffer.Length)
                {
                    _hashService.TransformBlock(buffer, 0, buffer.Length, null, 0);
                }
                else
                {
                    _hashService.TransformFinalBlock(buffer, 0, buffer.Length);
                }

                if (readed == 0)
                {
                    break;
                }
                fileStream.Write(buffer, 0, readed);
                bytesToCopyLeft -= readed;
            }
            return _hashService.Hash;
        }

        public bool EndSynchronization()
        {
            // todo Добавить фиксацию созданных файлов
            if (!_isSyncStarted)
            {
                return false; 
            }

            _unitOfWork.Save();
            _isSyncStarted = false;
            return true;
        }
    }

}
