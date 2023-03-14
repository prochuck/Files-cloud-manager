using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Services.Interfaces;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Files_cloud_manager.Client.Services
{
    internal class ServerConnectionService : IDisposable, IServerConnectionService
    {
        //todo сделать что-то с syncId
        // todo вынести syncId из serverconnection в programdatamodel
        public bool IsLoogedIn { get; private set; } = false;
        public bool IsSyncStarted { get;private set; } = false;
        /// <summary>
        /// Текущий ID синхронизации. -1, в случае если синхронизация не начата.
        /// </summary>
        private int _currentSyncId = -1;


        private Uri _baseAddress = new Uri("https://localhost:7216");
        private CookieContainer _cookieContainer;
        private string _login;
        private string _password;
        private HttpClient _httpClient;
        private FileCloudAPIClient _swaggerClient;
        private AsyncReaderWriterLock _coockieLock;

        private Timer _tokenRefreshTimer;



        public ServerConnectionService()
        {
            _cookieContainer = new CookieContainer();
            _httpClient = new System.Net.Http.HttpClient(new HttpClientHandler() { CookieContainer = _cookieContainer });
            _swaggerClient = new FileCloudAPIClient("https://localhost:7216", _httpClient);
            _coockieLock = new AsyncReaderWriterLock();
        }

        public async Task RefreshCoockie()
        {
            using (await _coockieLock.WriteLockAsync())
            {
                if (_cookieContainer.GetAllCookies().Where(e => e.Name == "FileCloudCoockie").First().Expires.CompareTo(DateTime.Now) < 0)
                {
                    await _swaggerClient.LoginAsync(_login, _password).ConfigureAwait(false);
                }
                else
                {
                    await _swaggerClient.RefreshCoockieAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task<bool> LoginAsync(string login, string password)
        {
            _login = login;
            _password = password;
            using (await _coockieLock.WriteLockAsync())
            {
                try
                {
                    await _swaggerClient.LoginAsync(login, password).ConfigureAwait(false);
                    TimeSpan timeBeforeExpiration = _cookieContainer.GetAllCookies().Where(e => e.Name == "FileCloudCoockie").First().Expires.Subtract(DateTime.Now);
                    _tokenRefreshTimer = new Timer(timeBeforeExpiration.Divide(2).TotalMilliseconds);
                    IsLoogedIn = true;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

        }
       
        public async Task<ICollection<FileInfoDTO>> GetFilesAsync()
        {
            if (!IsSyncStarted)
            {
                return null;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    return await _swaggerClient.GetFileInfoGroupContentAsync(_currentSyncId).ConfigureAwait(false);
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<bool> LogoutAsync()
        {
            using (await _coockieLock.WriteLockAsync())
            {
                try
                {
                    await _swaggerClient.LogoutAsync().ConfigureAwait(false);
                    IsLoogedIn = false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<bool> StartSynchronizationAsync(string groupName)
        {
            if (_currentSyncId != -1)
            {
                return false;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    _currentSyncId = await _swaggerClient.StartSynchronizationAsync(groupName).ConfigureAwait(false);
                    IsSyncStarted = true;
                    return true;
                }
                catch
                {
                    _currentSyncId = -1;
                    return false;
                }
            }
        }

        public async Task<bool> EndSyncAsync()
        {
            if (_currentSyncId == -1)
            {
                return false;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.EndSynchronizationAsync(_currentSyncId).ConfigureAwait(false);
                    IsSyncStarted=false;
                    _currentSyncId = -1;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public async Task<bool> RollBackSyncAsync()
        {
            if (_currentSyncId == -1)
            {
                return false;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.RollBackSynchronizationAsync(_currentSyncId).ConfigureAwait(false);
                    IsSyncStarted = false;
                    _currentSyncId = -1;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public async Task<bool> RollBackSyncAsync(string groupName)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.RollBackSynchronizationByNameAsync(groupName).ConfigureAwait(false);
                    IsSyncStarted = false;
                    _currentSyncId = -1;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public async Task<Stream> DonwloadFileAsync(string filePath,CancellationToken cancellationToken)
        {
            if (!IsSyncStarted)
            {
                return null;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    var fileResult = await _swaggerClient.GetFileAsync(_currentSyncId, filePath, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    return fileResult.Stream;
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<bool> CreateOrUpdateFileAsync(string filePath, Stream file,CancellationToken cancellationToken)
        {
            if (!IsSyncStarted)
            {
                return false;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.CreateOrUpdateFileAsync(_currentSyncId, filePath, new FileParameter(file), cancellationToken);


                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (!IsSyncStarted)
            {
                return false;
            }
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.DeleteFileAsync(_currentSyncId, filePath).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public async Task<ICollection<FileInfoGroupDTO>> GetFileInfoGroupsAsync()
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    return await _swaggerClient.GetFoldersListAsync().ConfigureAwait(false);
                }
                catch
                {
                    return null;
                }
            }
        }
        public void Dispose()
        {
            LogoutAsync().RunSynchronously();
        }
    }
}
