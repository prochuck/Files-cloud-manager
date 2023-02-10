using Files_cloud_manager.Models;
using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Files_cloud_manager.Server.Services
{
    public class SynchronizationContainerService : ISynchronizationContainerService
    {
        private ConcurrentDictionary<int, syncContext> _syncContexts = new ConcurrentDictionary<int, syncContext>();
        private HashSet<int> _usersWhithActiveSyncs = new HashSet<int>();

        private int lastId = 0;
        private object idLocker = new object();

        IServiceProvider _serviceProvider;

        struct syncContext
        {
            public IFilesSynchronizationService FileSyncService;
            public IServiceScope ServiceScope;
            public int UserId;
            public string FileGroupName;
        }

        public SynchronizationContainerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Начинает синхронизацию файлов
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fileGroupName"></param>
        /// <returns>Id синхронизации. -1 если сессия синхронизации не началась.</returns>
        public int StartNewSynchronization(int userId, string fileGroupName)
        {

            int id;
            lock (idLocker)
            {
                if (_usersWhithActiveSyncs.Contains(userId))
                {
                    return -1;
                }


                IServiceScope serviceScope = _serviceProvider.CreateScope();
                IFilesSynchronizationService fileSyncService = serviceScope.ServiceProvider.GetService<IFilesSynchronizationService>();

                if (!fileSyncService.StartSynchronization(userId, fileGroupName))
                {
                    serviceScope.Dispose();
                    return -1;
                }

                id = lastId;
                lastId++;

                _syncContexts.TryAdd(id, new syncContext
                {
                    UserId = userId,
                    FileGroupName = fileGroupName,
                    ServiceScope = serviceScope,
                    FileSyncService = fileSyncService
                });

                _usersWhithActiveSyncs.Add(userId);
                return id;
            }
        }

        public List<FileInfoDTO> GetFilesInfos(int userId, int syncId)
        {
            if (_syncContexts.ContainsKey(syncId) && _syncContexts[syncId].UserId == userId)
            {
                return _syncContexts[syncId].FileSyncService.GetFilesInfos();
            }
            return null;
        }

        public bool CreateOrUpdateFileInFileInfoGroup(int userId, int syncId, string filePath, Stream uploadedFile)
        {
            if (!_syncContexts.ContainsKey(syncId) || _syncContexts[syncId].UserId != userId)
            {
                return false;
            }
            if (_syncContexts[syncId].FileSyncService.CreateOrUpdateFile(filePath, uploadedFile))
            {
                return true;
            }
            return false;
        }

        public bool DeleteFileInFileInfoGroup(int userId, int syncId, string filePath)
        {
            if (!_syncContexts.ContainsKey(syncId) || _syncContexts[syncId].UserId != userId)
            {
                return false;
            }
            if (_syncContexts[syncId].FileSyncService.DeleteFile(filePath))
            {
                return true;
            }
            return false;
        }

        public Stream GetFile(int userId, int syncId, string filePath)
        {
            if (!_syncContexts.ContainsKey(syncId) || _syncContexts[syncId].UserId != userId)
            {
                return null;
            }
            Stream stream = _syncContexts[syncId].FileSyncService.GetFile(filePath);
            return stream;
        }
        /// <summary>
        /// Заканчивает синхронизацию
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="syncId">Id синхронизации, полученный в методе StartNewSynchronization</param>
        /// <returns></returns>
        public bool EndSynchronization(int userId, int syncId)
        {
            if (!(_syncContexts.ContainsKey(syncId) && _syncContexts[syncId].UserId == userId))
            {
                return false;
            }
            lock (_syncContexts[syncId].FileSyncService)
            {
                if (!(_syncContexts.ContainsKey(syncId) && _syncContexts[syncId].UserId == userId))
                {
                    return false;
                }
                if (_syncContexts[syncId].FileSyncService.EndSynchronization())
                {
                    _syncContexts[syncId].ServiceScope.Dispose();
                    _syncContexts.TryRemove(syncId, out _);
                    _usersWhithActiveSyncs.Remove(userId);
                    return true;
                }
            }

            return false;
        }

        public bool RollBackSynchronization(int userId, int syncId)
        {
            if (!(_syncContexts.ContainsKey(syncId) && _syncContexts[syncId].UserId == userId))
            {
                return false;
            }
            lock (_syncContexts[syncId].FileSyncService)
            {
                if (!(_syncContexts.ContainsKey(syncId) && _syncContexts[syncId].UserId == userId))
                {
                    return false;
                }
                if (_syncContexts[syncId].FileSyncService.RollBackSynchronization())
                {
                    _syncContexts[syncId].ServiceScope.Dispose();
                    _syncContexts.TryRemove(syncId, out _);
                    _usersWhithActiveSyncs.Remove(userId);
                    return true;
                }
            }

            return false;
        }


    }
}
