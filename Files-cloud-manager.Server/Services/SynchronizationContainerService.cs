using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Models.DTO;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Files_cloud_manager.Server.Services
{
    public class SynchronizationContainerService : ISynchronizationContainerService
    {
        const int rollBackTimeInMinutes = 30;

        private ConcurrentDictionary<int, SyncContext> _syncContexts = new ConcurrentDictionary<int, SyncContext>();
        private HashSet<int> _usersWhithActiveSyncs = new HashSet<int>();

        private int _lastId = 0;
        private SemaphoreSlim _createDeleteLock = new SemaphoreSlim(1);

        IServiceProvider _serviceProvider;

        struct SyncContext
        {
            public IFilesSynchronizationService FileSyncService;
            public IServiceScope ServiceScope;
            public int UserId;
            public string FileGroupName;
            public Timer Timer;
            public AsyncReaderWriterLock objectLocker;
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
        public async Task<int> StartNewSynchronizationAsync(int userId, string fileGroupName)
        {
            int id;

            await _createDeleteLock.WaitAsync();
            try
            {
                if (_usersWhithActiveSyncs.Contains(userId))
                {
                    return -1;
                }
                id = _lastId;
                _lastId++;
                _usersWhithActiveSyncs.Add(userId);
            }
            finally
            {
                _createDeleteLock.Release();
            }


            IServiceScope serviceScope = _serviceProvider.CreateScope();
            IFilesSynchronizationService fileSyncService = serviceScope.ServiceProvider.GetService<IFilesSynchronizationService>();

            if (!fileSyncService.StartSynchronization(userId, fileGroupName))
            {
                serviceScope.Dispose();
                await _createDeleteLock.WaitAsync();
                try
                {
                    _usersWhithActiveSyncs.Remove(userId);
                }
                finally
                {
                    _createDeleteLock.Release();
                }
                return -1;
            }

            SyncContext syncContext = new SyncContext
            {
                UserId = userId,
                FileGroupName = fileGroupName,
                ServiceScope = serviceScope,
                FileSyncService = fileSyncService,
                Timer = new Timer(TimeSpan.FromMinutes(rollBackTimeInMinutes).TotalMilliseconds),
                objectLocker = new AsyncReaderWriterLock()
            };
            syncContext.Timer.Elapsed += (e, k) => RollBackSynchronizationAsync(userId, id);

            await _createDeleteLock.WaitAsync();
            try
            {
                if (!_syncContexts.TryAdd(id, syncContext))
                {
                    return -1;
                }
                syncContext.Timer.Start();
            }
            finally
            {
                _createDeleteLock.Release();
            }
            return id;
        }


        public async Task<List<FileInfoDTO>> GetFilesInfosAsync(int userId, int syncId)
        {

            (AsyncReaderWriterLock.Releaser releaser, SyncContext syncContext) = await EnterSyncLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return null;
            }
            using (releaser)
            {
                syncContext.Timer.Stop();
                syncContext.Timer.Start();
                return syncContext.FileSyncService.GetFilesInfos();
            }
        }

        public async Task<bool> CreateOrUpdateFileInFileInfoGroupAsync(int userId, int syncId, string filePath, Stream uploadedFile)
        {
            (AsyncReaderWriterLock.Releaser releaser, SyncContext syncContext) = await EnterSyncLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return false;
            }
            using (releaser)
            {
                syncContext.Timer.Start();
                if (await syncContext.FileSyncService.CreateOrUpdateFileAsync(filePath, uploadedFile))
                {
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> DeleteFileInFileInfoGroupAsync(int userId, int syncId, string filePath)
        {
            (AsyncReaderWriterLock.Releaser releaser, SyncContext syncContext) = await EnterSyncLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return false;
            }
            using (releaser)
            {
                if (syncContext.FileSyncService.DeleteFile(filePath))
                {
                    syncContext.Timer.Stop();
                    syncContext.Timer.Start();
                    return true;
                }
                return false;
            }
        }

        public async Task<Stream> GetFileAsync(int userId, int syncId, string filePath)
        {
            (AsyncReaderWriterLock.Releaser releaser, SyncContext syncContext) = await EnterSyncLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return null;
            }
            using (releaser)
            {
                syncContext.Timer.Stop();
                syncContext.Timer.Start();
                Stream stream = syncContext.FileSyncService.GetFile(filePath);
                return stream;
            }
        }
        /// <summary>
        /// Заканчивает синхронизацию
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="syncId">Id синхронизации, полученный в методе StartNewSynchronization</param>
        /// <returns></returns>
        public async Task<bool> EndSynchronizationAsync(int userId, int syncId)
        {
            SyncContext syncContext;

            await _createDeleteLock.WaitAsync();
            try
            {
                if (!_syncContexts.TryGetValue(syncId, out syncContext) || syncContext.UserId != userId)
                {
                    return false;
                }

                using (await syncContext.objectLocker.WriteLockAsync())
                {
                    if (syncContext.FileSyncService.EndSynchronization())
                    {
                        syncContext.Timer.Dispose();
                        syncContext.ServiceScope.Dispose();
                        _syncContexts.TryRemove(syncId, out _);
                        _usersWhithActiveSyncs.Remove(userId);
                        return true;
                    }
                }
            }
            finally
            {
                _createDeleteLock.Release();
            }
            return false;
        }

        public async Task<bool> RollBackSynchronizationAsync(int userId, int syncId)
        {
            SyncContext syncContext;
            await _createDeleteLock.WaitAsync();
            try
            {
                if (!_syncContexts.TryGetValue(syncId, out syncContext) || syncContext.UserId != userId)
                {
                    return false;
                }

                using (await syncContext.objectLocker.WriteLockAsync())
                {
                    if (syncContext.FileSyncService.RollBackSynchronization())
                    {
                        syncContext.Timer.Stop();
                        syncContext.ServiceScope.Dispose();
                        _syncContexts.TryRemove(syncId, out _);
                        _usersWhithActiveSyncs.Remove(userId);
                        return true;
                    }
                }

            }
            finally
            {
                _createDeleteLock.Release();
            }
            return false;
        }

        enum LockTypes
        {
            ReadLock, WriteLock, UpgradeableLock
        }
        private async Task<(AsyncReaderWriterLock.Releaser, SyncContext)> EnterSyncLock(int syncId, int userId, LockTypes lockType)
        {
            SyncContext syncContext;
            await _createDeleteLock.WaitAsync();
            try
            {
                if (!_syncContexts.TryGetValue(syncId, out syncContext) || syncContext.UserId != userId)
                {
                    return (default(AsyncReaderWriterLock.Releaser), default(SyncContext));
                }
                switch (lockType)
                {
                    case LockTypes.ReadLock:
                        return (await syncContext.objectLocker.ReadLockAsync(), syncContext);
                        break;
                    case LockTypes.WriteLock:
                        return (await syncContext.objectLocker.ReadLockAsync(), syncContext);
                        break;
                    case LockTypes.UpgradeableLock:
                        return (await syncContext.objectLocker.UpgradeableReadLockAsync(), syncContext);
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                _createDeleteLock.Release();
            }
            return (default(AsyncReaderWriterLock.Releaser), default(SyncContext));
        }

    }
}
