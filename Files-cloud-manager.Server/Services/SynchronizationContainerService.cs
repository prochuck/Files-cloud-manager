using Files_cloud_manager.Models;
using Files_cloud_manager.Models.DTO;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        private int lastId = 0;
        private object createDeleteLock = new object();

        IServiceProvider _serviceProvider;

        struct SyncContext
        {
            public IFilesSynchronizationService FileSyncService;
            public IServiceScope ServiceScope;
            public int UserId;
            public string FileGroupName;
            public Timer Timer;
            public ReaderWriterLockSlim objectLocker;
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



            lock (createDeleteLock)
            {
                if (_usersWhithActiveSyncs.Contains(userId))
                {
                    return -1;
                }
                _usersWhithActiveSyncs.Add(userId);
                id = lastId;
                lastId++;
            }

            IServiceScope serviceScope = _serviceProvider.CreateScope();
            IFilesSynchronizationService fileSyncService = serviceScope.ServiceProvider.GetService<IFilesSynchronizationService>();

            if (!fileSyncService.StartSynchronization(userId, fileGroupName))
            {
                serviceScope.Dispose();
                lock (createDeleteLock)
                {
                    _usersWhithActiveSyncs.Remove(userId);
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
                objectLocker = new ReaderWriterLockSlim(),
            };
            _syncContexts[id].Timer.Elapsed += (e, k) => RollBackSynchronization(userId, id);

            lock (createDeleteLock)
            {
                if (!_syncContexts.TryAdd(id, syncContext))
                {
                    return -1;
                }
                _syncContexts[id].Timer.Start();
            }

            return id;
        }


        public List<FileInfoDTO> GetFilesInfos(int userId, int syncId)
        {
            SyncContext syncContext = GetSyncContextAndEnterLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return null;
            }
            try
            {
                syncContext.Timer.Stop();
                syncContext.Timer.Start();
                return syncContext.FileSyncService.GetFilesInfos();
            }
            finally
            {
                syncContext.objectLocker.ExitReadLock();
            }
        }

        public async Task<bool> CreateOrUpdateFileInFileInfoGroupAsync(int userId, int syncId, string filePath, Stream uploadedFile)
        {
            SyncContext syncContext = GetSyncContextAndEnterLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return false;
            }

            try
            {
                syncContext.Timer.Stop();
                if (await syncContext.FileSyncService.CreateOrUpdateFileAsync(filePath, uploadedFile))
                {
                    return true;
                }
                return false;
            }
            finally
            {
                syncContext.Timer.Start();
                syncContext.objectLocker.ExitReadLock();
            }
        }

        public bool DeleteFileInFileInfoGroup(int userId, int syncId, string filePath)
        {
            SyncContext syncContext = GetSyncContextAndEnterLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return false;
            }
            try
            {
                if (syncContext.FileSyncService.DeleteFile(filePath))
                {
                    syncContext.Timer.Stop();
                    syncContext.Timer.Start();
                    return true;
                }
                return false;
            }
            finally
            {
                syncContext.objectLocker.ExitReadLock();
            }
        }

        public Stream GetFile(int userId, int syncId, string filePath)
        {
            SyncContext syncContext = GetSyncContextAndEnterLock(syncId, userId, LockTypes.ReadLock);
            if (syncContext.Equals(default(SyncContext)))
            {
                return null;
            }
            try
            {
                syncContext.Timer.Stop();
                syncContext.Timer.Start();
                Stream stream = syncContext.FileSyncService.GetFile(filePath);
                return stream;
            }
            finally
            {
                syncContext.objectLocker.ExitReadLock();
            }
        }
        /// <summary>
        /// Заканчивает синхронизацию
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="syncId">Id синхронизации, полученный в методе StartNewSynchronization</param>
        /// <returns></returns>
        public bool EndSynchronization(int userId, int syncId)
        {
            SyncContext syncContext;
            lock (createDeleteLock)
            {
                if (!_syncContexts.TryGetValue(syncId, out syncContext) || syncContext.UserId != userId)
                {
                    return false;
                }
                try
                {
                    syncContext.objectLocker.EnterWriteLock();
                    if (syncContext.FileSyncService.EndSynchronization())
                    {
                        syncContext.Timer.Dispose();
                        syncContext.ServiceScope.Dispose();
                        _syncContexts.TryRemove(syncId, out _);
                        _usersWhithActiveSyncs.Remove(userId);
                        return true;
                    }
                }
                finally
                {
                    syncContext.objectLocker.ExitWriteLock();
                }
            }
            return false;
        }

        public bool RollBackSynchronization(int userId, int syncId)
        {
            SyncContext syncContext;
            lock (createDeleteLock)
            {
                if (!_syncContexts.TryGetValue(syncId, out syncContext) || syncContext.UserId != userId)
                {
                    return false;
                }
                try
                {
                    syncContext.objectLocker.EnterWriteLock();
                    if (syncContext.FileSyncService.RollBackSynchronization())
                    {
                        syncContext.Timer.Stop();
                        syncContext.ServiceScope.Dispose();
                        _syncContexts.TryRemove(syncId, out _);
                        _usersWhithActiveSyncs.Remove(userId);
                        return true;
                    }
                }
                finally
                {
                    syncContext.objectLocker.ExitWriteLock();
                }
            }
            return false;
        }

        enum LockTypes
        {
            ReadLock, WriteLock, UpgradeableLock
        }
        private SyncContext GetSyncContextAndEnterLock(int syncId, int userId, LockTypes lockType)
        {
            SyncContext syncContext;
            lock (createDeleteLock)
            {
                if (!_syncContexts.TryGetValue(syncId, out syncContext) || syncContext.UserId != userId)
                {
                    return default(SyncContext);
                }
                switch (lockType)
                {
                    case LockTypes.ReadLock:
                        syncContext.objectLocker.EnterReadLock();
                        break;
                    case LockTypes.WriteLock:
                        syncContext.objectLocker.EnterWriteLock();
                        break;
                    case LockTypes.UpgradeableLock:
                        syncContext.objectLocker.EnterUpgradeableReadLock();
                        break;
                    default:
                        break;
                }
            }
            return syncContext;
        }

    }
}
