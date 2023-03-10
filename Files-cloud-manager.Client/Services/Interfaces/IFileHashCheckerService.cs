using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Models;
using System.Collections.Generic;
using System.Threading;

namespace Files_cloud_manager.Client.Services.Interfaces
{
    internal interface IFileHashCheckerService
    {
        /// <summary>
        /// Сравнение переданных в функцию хэшей файлов и файлов на диске.
        /// </summary>
        /// <param name="fileInfos">Передаваемые файлыю</param>
        /// <param name="pathToFiles">Путь к файлам на диске.</param>
        /// <returns></returns>
        IAsyncEnumerable<FileDifferenceModel> GetHashDifferencesAsync(IEnumerable<FileInfoDTO> fileInfos, string pathToFiles, CancellationToken cancellationToken);
    }
}