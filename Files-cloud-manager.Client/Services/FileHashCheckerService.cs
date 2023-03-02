using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services
{
    internal class FileHashCheckerService
    {
        public async IAsyncEnumerable<FileDifferenceModel> GetHashDifferencesAsync(IEnumerable<FileInfoDTO> fileInfos,string basePath)
        {
            Dictionary<string, FileInfoDTO> pathToFileInfos = fileInfos.ToDictionary(e => e.RelativePath, e => e);

            IEnumerable<string> filesInDirectory = EnumerateAllFiles(basePath);

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

                byte[] bytes = await GetFileHashAsync(GetFullDataPath(basePath,item)).ConfigureAwait(false);

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
        public IEnumerable<string> EnumerateAllFiles(string path)
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
        public async Task<byte[]> GetFileHashAsync(string path)
        {
            HashAlgorithm hashAlgorithm = SHA256.Create();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                return await hashAlgorithm.ComputeHashAsync(stream).ConfigureAwait(false);
            }
        }
        public string GetFullDataPath(string basePath,string relativePath)
        {
            return $"{basePath}/{relativePath}";
        }
    }
}
