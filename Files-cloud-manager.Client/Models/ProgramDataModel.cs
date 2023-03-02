using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    //Алгоритмы хэширования приколы с ними
    class ProgramDataModel
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
        public async Task SynchronizeFiles(List<FileDifferenceModel> fileDifferenceModels, SyncDirection syncDirection)
        {
            int syncId = await _connectionService.StartSynchronizationAsync(GroupName);
            try
            {
                foreach (var fileDiff in fileDifferenceModels)
                {
                }
            }
            catch (Exception)
            {
                _connectionService.RollBackSync(syncId);
            }
        }
        private async Task<List<FileDifferenceModel>> CompareLocalFilesToServerAsync()
        {
            int id = await _connectionService.StartSynchronizationAsync("fol1").ConfigureAwait(false);
            if (id == -1)
            {
                return null;
            }
            ICollection<FileInfoDTO> serverFiles = (await _connectionService.GetFiles(id).ConfigureAwait(false));
            if (serverFiles is null)
            {
                return null;
            }
            List<FileDifferenceModel> res = new List<FileDifferenceModel>();
            await foreach (var item in _fileHashCheckerService.GetHashDifferencesAsync(serverFiles, PathToData).ConfigureAwait(false))
            {
                res.Add(item);
            }
            return res;
        }



        //todo вынести приколы с файлами в отдельный сервис

    }
}
