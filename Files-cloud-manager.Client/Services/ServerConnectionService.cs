using FileCloudAPINameSpace;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Timer = System.Timers.Timer;

namespace Files_cloud_manager.Client.Services
{
    internal class ServerConnectionService
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

            /*
            Task a = _swaggerClient.LoginAsync("admin", "123");
            a.Wait();
            foreach (var item in _cookieContainer.GetAllCookies().ToArray())
            {
                Thread.Sleep(2000);
                Console.WriteLine(item.Expired);
            }
            var res = _swaggerClient.GetFoldersListAsync().Result;*/
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

        public async Task<bool> Login(string login, string password)
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

        public async Task<ICollection<FileInfoDTO>> GetFiles(int syncId)
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

        public async Task<bool> EndSync(int syncId)
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
        public async Task<bool> RollBackSync(int syncId)
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

        public async Task<Stream> DonwloadFile(int syncId, string filePath)
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

        public async Task<bool> CreateOrUpdateFile(int syncId, string filePath, Stream file)
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

        public async Task<bool> DeleteFile(int syncId, string filePath)
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

        private string CreateQuery(string requestUri, Dictionary<string, string> keysValuesForQuery)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var item in keysValuesForQuery)
            {
                query[item.Key] = item.Value;
            }
            return $"{requestUri}?{query.ToString()}";
        }
    }
}
