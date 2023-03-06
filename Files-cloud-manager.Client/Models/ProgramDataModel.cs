using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    //Алгоритмы хэширования приколы с ними
    class ProgramDataModel : IDisposable
    {
        //todo сделать синхронизацию алгоритмов хэширования с сервером
        //todo добавить DI 
        public string PathToExe { get; set; }
        public string PathToData { get; set; }
        public string GroupName { get; set; }

        private ServerConnectionService _connectionService;
        private FileHashCheckerService _fileHashCheckerService;

        public ProgramDataModel(ServerConnectionService connectionService, FileHashCheckerService fileHashCheckerService)
        {
            _connectionService = connectionService;
            _fileHashCheckerService = fileHashCheckerService;
        }


        public async Task<bool> SynchronizeFiles(SyncDirection syncDirection)
        {
            // todo добавить cancelToken.
            int syncId = await _connectionService.StartSynchronizationAsync(GroupName).ConfigureAwait(false);

            if (syncId == -1)
            {
                return false;
            }

            IAsyncEnumerable<FileDifferenceModel> fileDifferenceModels = CompareLocalFilesToServerAsync(syncId);

            try
            {
                List<Task> fileDonwloads = new List<Task>();
                await foreach (var fileDiff in fileDifferenceModels.ConfigureAwait(false))
                {
                    fileDonwloads.Add(SynchronizeFile(syncDirection, fileDiff, syncId));
                }

                await Task.WhenAll(fileDonwloads).ConfigureAwait(false);
            }
            catch
            {
                await _connectionService.RollBackSyncAsync(syncId).ConfigureAwait(false);
                return false;
            }
            await _connectionService.EndSyncAsync(syncId).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> SynchronizeFile(SyncDirection direction, FileDifferenceModel file, int syncId)
        {
            switch (direction)
            {
                case SyncDirection.FromServer:
                    if (file.State != FileState.ServerOnly)
                    {
                        await Task.Run(() => File.Delete(GetFullDataPath(file.File.RelativePath))).ConfigureAwait(false);
                    }
                    if (file.State != FileState.ClientOnly)
                    {
                        Stream stream = await _connectionService.DonwloadFileAsync(syncId, file.File.RelativePath).ConfigureAwait(false);

                        using (FileStream newFile = File.Create(GetFullDataPath(file.File.RelativePath)))
                        {
                            await stream.CopyToAsync(newFile).ConfigureAwait(false);
                        }
                    }
                    return true;
                    break;
                case SyncDirection.FromClient:
                    if (file.State == FileState.ServerOnly)
                    {
                        return await _connectionService.DeleteFileAsync(syncId, file.File.RelativePath).ConfigureAwait(false);
                    }
                    else
                    {
                        using (FileStream fileStream = new FileStream(
                            GetFullDataPath(file.File.RelativePath),
                            FileMode.Open, FileAccess.Read,
                            FileShare.Read,
                            4096,
                            true))
                        {
                            return await _connectionService.CreateOrUpdateFileAsync(syncId, file.File.RelativePath, fileStream).ConfigureAwait(false);
                        }
                    }
                    break;
                default:
                    return false;
                    break;
            }
        }

        public async IAsyncEnumerable<FileDifferenceModel> CompareLocalFilesToServerAsync(int syncId)
        {
            if (syncId == -1)
            {
                yield break;
            }
            ICollection<FileInfoDTO> serverFiles = (await _connectionService.GetFilesAsync(syncId).ConfigureAwait(false));
            if (serverFiles is null)
            {
                yield break;
            }

            await foreach (var item in _fileHashCheckerService.GetHashDifferencesAsync(serverFiles, PathToData).ConfigureAwait(false))
            {
                yield return item;
            }
        }

        public string GetFullDataPath(string relativePath)
        {
            return $"{PathToData}/{relativePath}";
        }

        public void Dispose()
        {
            _connectionService.Dispose();
        }

        //todo вынести приколы с файлами в отдельный сервис

    }
    public enum SyncDirection
    {
        /// <summary>
        /// Файлы сервера будут выбраны как актуальные.
        /// </summary>
        FromServer,
        /// <summary>
        /// Файлы клиента будут выбраны как актуальные.
        /// </summary>
        FromClient
    }
}
