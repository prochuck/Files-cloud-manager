using FileCloudAPINameSpace;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Files_cloud_manager.Client.Services
{
    internal class ServerConnectionService : IDisposable, IServerConnectionService
    {
        //todo сделать что-то с syncId
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

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<ICollection<FileInfoDTO>> GetFilesAsync(int syncId)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    return await _swaggerClient.GetFileInfoGroupContentAsync(syncId).ConfigureAwait(false);
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
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<int> StartSynchronizationAsync(string groupName)
        {
            int res = -1;
            using (await _coockieLock.ReadLockAsync())
            {

                try
                {

                    return await _swaggerClient.StartSynchronizationAsync(groupName).ConfigureAwait(false);
                }
                catch
                {
                    return -1;
                }

            }
            return res;
        }

        public async Task<bool> EndSyncAsync(int syncId)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.EndSynchronizationAsync(syncId).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public async Task<bool> RollBackSyncAsync(int syncId)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.RollBackSynchronizationAsync(syncId).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<Stream> DonwloadFileAsync(int syncId, string filePath)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    var fileResult = await _swaggerClient.GetFileAsync(syncId, filePath).ConfigureAwait(false);
                    return fileResult.Stream;
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<bool> CreateOrUpdateFileAsync(int syncId, string filePath, Stream file)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.CreateOrUpdateFileAsync(syncId, filePath, new FileParameter(file));


                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<bool> DeleteFileAsync(int syncId, string filePath)
        {
            using (await _coockieLock.ReadLockAsync())
            {
                try
                {
                    await _swaggerClient.DeleteFileAsync(syncId, filePath).ConfigureAwait(false);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            LogoutAsync().RunSynchronously();
        }
    }
}
