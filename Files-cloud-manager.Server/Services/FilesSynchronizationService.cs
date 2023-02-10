using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using System.IO;
using System.Security.Cryptography;
using FileInfo = Files_cloud_manager.Models.FileInfo;
using AutoMapper;
using Files_cloud_manager.Server.Domain;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Files_cloud_manager.Server.Configs;

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
        private string _tmpFilesPath;
        private HashAlgorithm _hashService;
        private IMapper _mapper;

        public FilesSynchronizationService(IUnitOfWork unitOfWork, IOptions<FilesSyncServiceConfig> config, IMapper mapper, HashAlgorithm hashService)
        {
            _unitOfWork = unitOfWork;

            _basePath = config.Value.BaseFilesPath;
            _tmpFilesPath = config.Value.TmpFilesPath;

            _mapper = mapper;
            _hashService = hashService;
            _hashService.Initialize();
        }

        public bool StartSynchronization(int userId, string fileInfoGroupName)
        {
            _fileInfoGroup = _unitOfWork.FileInfoGroupRepostiory.Get(
                    e => e.Name == fileInfoGroupName && e.OwnerId == userId,
                    new string[] { nameof(_fileInfoGroup.Files), nameof(_fileInfoGroup.Owner) }
            ).FirstOrDefault();

            if (_fileInfoGroup is null || _fileInfoGroup.OwnerId != userId)
            {
                return false;
            }

            _isSyncStarted = true;
            _filesInfos = _fileInfoGroup.Files.ToDictionary(e => e.RelativePath);
            return true;
        }
        public bool CreateOrUpdateFile(string filePath, Stream originalFileStream)
        {
            if (!_isSyncStarted)
            {
                return false;
            }
            if (!CheckIfFileNameIsValid(filePath))
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
            if (!_isSyncStarted)
            {
                return false;
            }
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

            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open);
            }
            return null;
        }
        public List<FileInfoDTO> GetFilesInfos()
        {
            return _fileInfoGroup.Files.Select(e => _mapper.Map<FileInfo, FileInfoDTO>(e)).ToList();
        }

        private string GetFullPath(string relativePath)
        {
            return $"{_basePath}/{_fileInfoGroup.Owner.Login}/{_fileInfoGroup.Name}/{relativePath}";
        }
        private byte[] CreateFileFromStream(Stream originalFileStream, string path)
        {
            lock (_hashService)
            {
                _hashService.Initialize();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (FileStream fileStream = new FileStream(path, FileMode.CreateNew))
                {
                    long bytesToCopyLeft = originalFileStream.Length;
                    byte[] buffer = new byte[1024];
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
                }
                return _hashService.Hash;
            }
        }
        private bool CheckIfFileNameIsValid(string name)
        {
            return !name.IsNullOrEmpty() &&
                name.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        }
        public bool EndSynchronization()
        {
            // todo Добавить фиксацию созданных файлов, и возможность отката
            if (!_isSyncStarted)
            {
                return false;
            }

            _unitOfWork.Save();
            _isSyncStarted = false;
            return true;
        }

        public bool RollBackSynchronization()
        {
            if (!_isSyncStarted)
            {
                return false;
            }

            _isSyncStarted = false;

            return true;
        }

    }

}
