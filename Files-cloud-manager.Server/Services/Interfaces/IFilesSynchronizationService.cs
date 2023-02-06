﻿namespace Files_cloud_manager.Server.Services.Interfaces
{
    public interface IFilesSynchronizationService
    {
        bool CreateOrUpdateFile(string filePath, Stream originalFileStream);
        bool DeleteFile(string filePath);
        bool EndSynchronization();
        Stream GetFile(string filePath);
        bool StartSynchronization(int userId, int folderId);
    }
}