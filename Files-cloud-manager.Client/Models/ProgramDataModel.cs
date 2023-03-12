using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Files_cloud_manager.Client.Models
{
    //Алгоритмы хэширования приколы с ними
    class ProgramDataModel : IDisposable, IValidatableObject
    {
        // todo сделать синхронизацию алгоритмов хэширования с сервером
        // todo Сделать откат изменений на клиенте.
        public string PathToExe { get; set; }
        public string PathToData { get; set; }
        public string GroupName { get; }
        public ReadOnlyObservableCollection<FileDifferenceModel> FileDifferences { get; private set; }

        private bool _isFileDiffCollectionSync = false;

        private ObservableCollection<FileDifferenceModel> _fileDifferences;
       
        private SemaphoreSlim _fileDifferencesCollectionLock = new SemaphoreSlim(1);

        private IServerConnectionService _connectionService;
        private IFileHashCheckerService _fileHashCheckerService;

        private object _locker = new object();

        public ProgramDataModel(
            IServerConnectionService connectionService,
            IFileHashCheckerService fileHashCheckerService,
            string pathToExe,
            string pathToData,
            string groupName)
        {

            _connectionService = connectionService;
            _fileHashCheckerService = fileHashCheckerService;

            _fileDifferences = new ObservableCollection<FileDifferenceModel>();
            FileDifferences = new ReadOnlyObservableCollection<FileDifferenceModel>(_fileDifferences);

            PathToExe = pathToExe;
            PathToData = pathToData;
            GroupName = groupName;

          

        }


        public async Task<bool> SynchronizeFilesAsync(SyncDirection syncDirection, CancellationToken cancellationToken)
        {
            // todo добавить cancelToken.
            if (!await EnsureSyncStartedAsync().ConfigureAwait(false))
            {
                return false;
            }

            if (!_isFileDiffCollectionSync)
            {
                await CompareLocalFilesToServerAsync(cancellationToken);
            }

            await _fileDifferencesCollectionLock.WaitAsync();

            try
            {
                List<Task> fileDonwloads = new List<Task>();
                foreach (var fileDiff in _fileDifferences)
                {
                    fileDonwloads.Add(SynchronizeFileAsync(syncDirection, fileDiff, cancellationToken));
                }

                await Task.WhenAll(fileDonwloads).ConfigureAwait(false);
            }
            catch
            {
                await _connectionService.RollBackSyncAsync().ConfigureAwait(false);
                throw;
            }
            finally
            {
                _fileDifferencesCollectionLock.Release();
            }
            await _connectionService.EndSyncAsync().ConfigureAwait(false);
            _fileDifferences.Clear();
            _isFileDiffCollectionSync = false;
            return true;
        }

        private async Task<bool> EnsureSyncStartedAsync()
        {
            if (!_connectionService.IsSyncStarted)
            {
                await _connectionService.StartSynchronizationAsync(GroupName).ConfigureAwait(false);
                if (!_connectionService.IsSyncStarted)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<ReadOnlyObservableCollection<FileDifferenceModel>> CompareLocalFilesToServerAsync(CancellationToken cancellationToken)
        {
            if (!await EnsureSyncStartedAsync().ConfigureAwait(false))
            {
                return FileDifferences;
            }

            await _fileDifferencesCollectionLock.WaitAsync();
            try
            {
                _fileDifferences.Clear();
                _isFileDiffCollectionSync = false;
                ICollection<FileInfoDTO> serverFiles = (await _connectionService.GetFilesAsync().ConfigureAwait(false));
                if (serverFiles is not null)
                {
                    await foreach (var item in _fileHashCheckerService.GetHashDifferencesAsync(serverFiles, PathToData, cancellationToken).ConfigureAwait(false))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _fileDifferences.Add(item); 
                    }
                }
                _isFileDiffCollectionSync = true;
                return FileDifferences;
            }
            finally
            {
                _fileDifferencesCollectionLock.Release();
            }
        }


        private async Task<bool> SynchronizeFileAsync(SyncDirection direction, FileDifferenceModel file, CancellationToken cancellationToken)
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
                        Stream stream = await _connectionService.DonwloadFileAsync(file.File.RelativePath, cancellationToken).ConfigureAwait(false);

                        using (FileStream newFile = File.Create(GetFullDataPath(file.File.RelativePath), 4096, FileOptions.Asynchronous))
                        {
                            await stream.CopyToAsync(newFile, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    return true;
                    break;
                case SyncDirection.FromClient:
                    if (file.State == FileState.ServerOnly)
                    {
                        return await _connectionService.DeleteFileAsync(file.File.RelativePath).ConfigureAwait(false);
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
                            return await _connectionService.CreateOrUpdateFileAsync(file.File.RelativePath, fileStream, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    break;
                default:
                    return false;
                    break;
            }
        }



        private string GetFullDataPath(string relativePath)
        {
            return $"{PathToData}/{relativePath}";
        }

        public void Dispose()
        {
            _connectionService.RollBackSyncAsync().RunSynchronously();
            _connectionService.Dispose();
        }
        // todo доделать
        /// <summary>
        /// Не доделано.
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckIfFileNameIsValid(PathToExe) || !File.Exists(PathToExe))
                yield return new ValidationResult("Путь к exe не верен");
            if (CheckIfFileNameIsValid(PathToData) || !Directory.Exists(PathToData))
                yield return new ValidationResult("Путь к данным не верен");
        }
        // todo сделать проверку имени файла лучше
        private bool CheckIfFileNameIsValid(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                name.IndexOfAny(Path.GetInvalidPathChars()) < 0;
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
