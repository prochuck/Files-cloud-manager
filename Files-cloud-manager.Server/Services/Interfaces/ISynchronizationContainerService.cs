using Files_cloud_manager.Models.DTO;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    public interface ISynchronizationContainerService
    {
        bool CreateOrUpdateFileInFileInfoGroup(int userId, int syncId, string filePath, Stream uploadedFile);
        bool DeleteFileInFileInfoGroup(int userId, int syncId, string filePath);
        bool EndSynchronization(int userId, int syncId);
        Stream GetFile(int userId, int syncId, string filePath);
        List<FileInfoDTO> GetFilesInfos(int userId, int syncId);
        bool RollBackSynchronization(int userId, int syncId);
        int StartNewSynchronization(int userId, string fileGroupName);
    }
}