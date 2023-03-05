using Files_cloud_manager.Server.Models.DTO;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    public interface ISynchronizationContainerService
    {
        Task<bool> CreateOrUpdateFileAsync(int userId, int syncId, string filePath, Stream uploadedFile);
        Task<bool> DeleteFileAsync(int userId, int syncId, string filePath);
        Task<bool> EndSynchronizationAsync(int userId, int syncId);
        Task<Stream> GetFileAsync(int userId, int syncId, string filePath);
        Task<List<FileInfoDTO>> GetFilesInfosAsync(int userId, int syncId);
        Task<bool> RollBackSynchronizationAsync(int userId, int syncId);
        Task<int> StartNewSynchronizationAsync(int userId, string fileGroupName);
    }
}