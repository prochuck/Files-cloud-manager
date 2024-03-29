﻿using Files_cloud_manager.Server.Models.DTO;
using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using System.IO;
using System.Security.Cryptography;
using FileInfo = Files_cloud_manager.Server.Models.FileInfo;
using AutoMapper;
using Files_cloud_manager.Server.Domain;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Files_cloud_manager.Server.Configs;
using System.Collections.Concurrent;

namespace Files_cloud_manager.Server.Services
{
    public class FilesSynchronizationService : IFilesSynchronizationService
    {
        // todo сделать проверку имени файла лучше

        /// <summary>
        /// Словарь путь к файлу - файл
        /// </summary>
        private ConcurrentDictionary<string, FileInfo> _filesInfos;
        private FileInfoGroup? _fileInfoGroup;
        private ConcurrentBag<string> _changedFiles = new ConcurrentBag<string>();

        private bool _isSyncStarted;

        private string _basePath;
        private string _tmpFilesPath;

        private IHashAlgorithmFactory _hashAlgFactory;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;

        private ConcurrentDictionary<string, SemaphoreSlim> _semaphoresForFiles = new ConcurrentDictionary<string, SemaphoreSlim>();
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public FilesSynchronizationService(IUnitOfWork unitOfWork,
            IOptions<FilesSyncServiceConfig> config,
            IMapper mapper,
            IHashAlgorithmFactory hashAlgFactory)
        {
            _unitOfWork = unitOfWork;

            _basePath = config.Value.BaseFilesPath;
            _tmpFilesPath = config.Value.TmpFilesPath;

            _mapper = mapper;
            _hashAlgFactory = hashAlgFactory;
        }

        public bool StartSynchronization(int userId, string fileInfoGroupName)
        {
            lock (_semaphoresForFiles)
            {
                if (_isSyncStarted)
                {
                    return false;
                }
                _fileInfoGroup = _unitOfWork.FileInfoGroupRepostiory.Get(
                        e => e.Name == fileInfoGroupName && e.OwnerId == userId,
                        new string[] { nameof(_fileInfoGroup.Files), nameof(_fileInfoGroup.Owner) }
                ).FirstOrDefault();

                if (_fileInfoGroup is null || _fileInfoGroup.OwnerId != userId)
                {
                    return false;
                }

                _isSyncStarted = true;
                _filesInfos = new ConcurrentDictionary<string, FileInfo>(_fileInfoGroup.Files.ToDictionary(e => e.RelativePath));
                return true;
            }
        }
        /// <summary>
        /// Создание или изменение файла, находящегося на пути.
        /// Один файл может быть изменён или создан лишь 1 раз за синхронизацию.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <param name="originalFileStream">Поток файлу.</param>
        /// <returns></returns>
        public async Task<bool> CreateOrUpdateFileAsync(string filePath, Stream originalFileStream)
        {
            SemaphoreSlim semaphoreSlim = GetSemaphoreForFilePath(filePath);
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!_isSyncStarted || !CheckIfFileNameIsValid(filePath))
                {
                    return false;
                }

                _changedFiles.Add(filePath);
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

                string fullPath = GetFullPath(filePath);
                if (File.Exists(fullPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(GetFullTmpPath(filePath)));
                    File.Move(fullPath, GetFullTmpPath(filePath));
                }
                try
                {
                    fileInfo.Hash = await CreateFileFromStreamAsync(originalFileStream, fullPath, _cancellationTokenSource.Token);
                }
                catch
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        if (File.Exists(GetFullPath(filePath)))
                        {
                            File.Delete(GetFullPath(filePath));
                        }
                    }
                    throw;
                }

                if (!_filesInfos.ContainsKey(filePath))
                {
                    _filesInfos.TryAdd(filePath, fileInfo);
                    _unitOfWork.FileInfoRepository.Create(fileInfo);
                }
                else
                {
                    _unitOfWork.FileInfoRepository.Update(fileInfo);
                }

                return true;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        /// <summary>
        /// Удаление файла. Каждый путь может быть использован только 1 раз.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool DeleteFile(string filePath)
        {
            SemaphoreSlim semaphoreSlim = GetSemaphoreForFilePath(filePath);

            semaphoreSlim.Wait();
            try
            {
                if (!_isSyncStarted || !_filesInfos.ContainsKey(filePath))
                {
                    return false;
                }

                FileInfo fileInfo = _filesInfos[filePath];
                string fullPath = GetFullPath(filePath);

                if (File.Exists(fullPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(GetFullTmpPath(filePath)));
                    File.Move(fullPath, GetFullTmpPath(filePath));
                }
                else
                {
                    return false;
                }

                _changedFiles.Add(filePath);
                _filesInfos.TryRemove(filePath, out _);
                _unitOfWork.FileInfoRepository.Delete(fileInfo);

                return true;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        /// <summary>
        /// Чтение файла.
        /// Файлы, которые были изменены в процессе синхронизации прочитать не получится.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Stream GetFile(string filePath)
        {
            SemaphoreSlim semaphoreSlim = GetSemaphoreForFilePath(filePath);
            semaphoreSlim.Wait();
            try
            {
                if (!_isSyncStarted || !_filesInfos.ContainsKey(filePath))
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
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public List<FileInfoDTO> GetFilesInfos()
        {
            return _fileInfoGroup.Files.Select(e => _mapper.Map<FileInfo, FileInfoDTO>(e)).ToList();
        }
        public bool EndSynchronization()
        {
            lock (_semaphoresForFiles)
            {
                if (!_isSyncStarted)
                {
                    return false;
                }
                foreach (var item in _semaphoresForFiles.Values)
                {
                    // Проверка того, что все операции с файлами завершены.
                    item.Wait();
                }

                if (Directory.Exists(GetFullTmpPath("")))
                {
                    Directory.Delete(GetFullTmpPath(""), true);
                }

                _unitOfWork.Save();
                _isSyncStarted = false;
                return true;
            }
        }
        public bool RollBackSynchronization()
        {
            lock (_semaphoresForFiles)
            {
                if (!_isSyncStarted)
                {
                    return false;
                }
                _cancellationTokenSource.Cancel();
                foreach (var item in _semaphoresForFiles.Values)
                {
                    // Проверка того, что все операции с файлами завершены.
                    item.Wait();
                }

                foreach (string item in _changedFiles)
                {
                    if (File.Exists(GetFullPath(item)))
                    {
                        File.Delete(GetFullPath(item));
                    }
                    if (File.Exists(GetFullTmpPath(item)))
                    {
                        File.Move(GetFullTmpPath(item), GetFullPath(item));
                    }
                }
                _isSyncStarted = false;
            }
            return true;
        }

        private string GetFullPath(string relativePath)
        {
            return $"{_basePath}/{_fileInfoGroup.Owner.Login}/{_fileInfoGroup.Name}/{relativePath}";
        }
        private string GetFullTmpPath(string relativePath)
        {
            return $"{_tmpFilesPath}/{_fileInfoGroup.Owner.Login}/{_fileInfoGroup.Name}/{relativePath}";
        }
        private async Task<byte[]> CreateFileFromStreamAsync(Stream originalFileStream, string path, CancellationToken cancellationToken)
        {
            HashAlgorithm hashAlgorithm = _hashAlgFactory.Create();
            hashAlgorithm.Initialize();
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            using (CryptoStream cryptoStream = new CryptoStream(fileStream, hashAlgorithm, CryptoStreamMode.Write))
            {
                await originalFileStream.CopyToAsync(cryptoStream, cancellationToken);
            }
            return hashAlgorithm.Hash;

        }

        private SemaphoreSlim GetSemaphoreForFilePath(string filePath)
        {
            lock (_semaphoresForFiles)
            {
                if (!_isSyncStarted)
                    throw new Exception("Попытка изменить файл без начала синхронизации");
                if (!_semaphoresForFiles.ContainsKey(filePath))
                {
                    _semaphoresForFiles.TryAdd(filePath, new SemaphoreSlim(1));
                }
                return _semaphoresForFiles[filePath];
            }
        }

        /// <summary>
        /// Получить доступ к файлу с путём. К каждому файлу может быть получен доступ только 1 раз.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>


        private bool CheckIfFileNameIsValid(string name)
        {
            return !name.IsNullOrEmpty() &&
                name.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        }
    }

}
