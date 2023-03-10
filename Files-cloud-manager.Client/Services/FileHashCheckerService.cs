using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services
{
    internal class FileHashCheckerService : IFileHashCheckerService
    {
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        public async IAsyncEnumerable<FileDifferenceModel> GetHashDifferencesAsync(
            IEnumerable<FileInfoDTO> fileInfos,
            string pathToFiles,
            CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Dictionary<string, FileInfoDTO> pathToFileInfos = fileInfos.ToDictionary(e => e.RelativePath, e => e);

                IEnumerable<string> filesInDirectory = EnumerateAllFiles(pathToFiles);

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
                            State = FileState.ClientOnly
                        };
                        continue;
                    }

                    byte[] bytes = await GetFileHashAsync(GetFullDataPath(pathToFiles, item), cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!bytes.SequenceEqual(pathToFileInfos[item].Hash))
                    {
                        yield return new FileDifferenceModel() { File = pathToFileInfos[item], State = FileState.Modified };
                    }
                    pathToFileInfos.Remove(item);
                }
                foreach (var item in pathToFileInfos)
                {
                    yield return new FileDifferenceModel() { File = item.Value, State = FileState.ServerOnly };
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        private async Task<byte[]> GetFileHashAsync(string path, CancellationToken cancellationToken)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                return await hashAlgorithm.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
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
        private string GetFullDataPath(string basePath, string relativePath)
        {
            return $"{basePath}/{relativePath}";
        }
    }
}
