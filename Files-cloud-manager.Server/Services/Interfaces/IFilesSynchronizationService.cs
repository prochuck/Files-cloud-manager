using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Models.DTO;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    public interface IFilesSynchronizationService
    {
        Task<bool> CreateOrUpdateFileAsync(string filePath, Stream originalFileStream);
        bool DeleteFile(string filePath);
        bool EndSynchronization();
        Stream GetFile(string filePath);
        public List<FileInfoDTO> GetFilesInfos();
        bool StartSynchronization(int userId, string fileInfoGroupName);
        bool RollBackSynchronization();
    }
}