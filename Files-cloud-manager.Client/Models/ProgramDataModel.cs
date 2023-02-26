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

        private ServerConnectionService _connectionService;

        public ProgramDataModel(ServerConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task<List<FileDifferenceModel>> CompareLocalFilesToServer()
        {
            int id = await _connectionService.StartSyncAsync("fol1").ConfigureAwait(false);
            if (id == -1)
            {
                return null;
            }
            List<FileInfoDTO> serverFiles = await _connectionService.GetFiles(id).ConfigureAwait(false) as List<FileInfoDTO>;
            if (serverFiles is null)
            {
                return null;
            }
            List<FileDifferenceModel> res = new List<FileDifferenceModel>();
            await foreach (var item in GetHashDifferencesAsync(serverFiles).ConfigureAwait(false))
            {
                res.Add(item);
            }
            return res;
        }

        //todo вынести приколы с файлами в отдельный сервис
        private async IAsyncEnumerable<FileDifferenceModel> GetHashDifferencesAsync(List<FileInfoDTO> fileInfos)
        {
            Dictionary<string, FileInfoDTO> pathToFileInfos = fileInfos.ToDictionary(e => e.RelativePath, e => e);

            IEnumerable<string> filesInDirectory = EnumerateAllFiles(PathToData);

            foreach (var item in filesInDirectory)
            {
                if (!pathToFileInfos.ContainsKey(item))
                {
                    yield return new FileDifferenceModel()
                    {
                        File = new FileInfoDTO()
                        {
                            RelativePath = item
                        },
                        State = FileState.Created
                    };
                    continue;
                }

                byte[] bytes = await GetFileHashAsync(GetFullDataPath(item)).ConfigureAwait(false);

                if (!bytes.SequenceEqual(pathToFileInfos[item].Hash))
                {
                    yield return new FileDifferenceModel() { File = pathToFileInfos[item], State = FileState.Modified };
                    pathToFileInfos.Remove(item);
                    continue;
                }
            }
            foreach (var item in pathToFileInfos)
            {
                yield return new FileDifferenceModel() { File = item.Value, State = FileState.Deleted };
            }
        }
        private IEnumerable<string> EnumerateAllFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new Exception($"Отсутвувет папка {path}");
            }

            Queue<string> directorysToVisit = new Queue<string>();
            directorysToVisit.Enqueue(path);


            while (directorysToVisit.Count != 0)
            {
                string directoryToVisit = directorysToVisit.Dequeue();
                try
                {
                    foreach (var subDir in Directory.EnumerateDirectories(directoryToVisit))
                    {
                        directorysToVisit.Enqueue(subDir);
                    }
                }
                catch { }
                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(directoryToVisit);
                }
                catch { continue; }
                foreach (var file in files)
                {
                    yield return Path.GetRelativePath(path, file);
                }
            }

        }
        private async Task<byte[]> GetFileHashAsync(string path)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                return await hashAlgorithm.ComputeHashAsync(stream).ConfigureAwait(false);
            }
        }
        private string GetFullDataPath(string relativePath)
        {
            return $"{PathToData}/{relativePath}";
        }
    }
}
