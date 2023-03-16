using Files_cloud_manager.Server.Models.DTO;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    /// <summary>
    /// Сервис, хранящий информацию о синхронизациях
    /// </summary>
    public interface ISynchronizationContainerService
    {
        /// <summary>
        /// Создать или заменить существующий файл.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="syncId">id синхронизации</param>
        /// <param name="filePath">относительный путь к файлу</param>
        /// <param name="uploadedFile">Загруженный файл</param>
        /// <returns></returns>
        Task<bool> CreateOrUpdateFileAsync(int userId, int syncId, string filePath, Stream uploadedFile);
        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="syncId">id синхронизации</param>
        /// <param name="filePath">относительный путь к файлу</param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(int userId, int syncId, string filePath);
        /// <summary>
        /// Закончить синхронизацию и применить изменения.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="syncId">id синхронизации</param>
        /// <returns></returns>
        Task<bool> EndSynchronizationAsync(int userId, int syncId);
        /// <summary>
        /// Получить файл.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="syncId">id синхронизации</param>
        /// <param name="filePath">относительный путь к файлу</param>
        /// <returns></returns>
        Task<Stream> GetFileAsync(int userId, int syncId, string filePath);
        /// <summary>
        /// Получить список файлов в группе.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="syncId">id синхронизации</param>
        /// <returns></returns>
        Task<List<FileInfoDTO>> GetFilesInfosAsync(int userId, int syncId);
        /// <summary>
        /// Отменить синхронизацию и откатить изменения.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="syncId">id синхронизации</param>
        /// <returns></returns>
        Task<bool> RollBackSynchronizationAsync(int userId, int syncId);
        /// <summary>
        /// Отменить синхронизацию по имени группы и откатить изменения.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="fileGroupName">Имя файловой группы</param>
        /// <returns></returns>
        Task<bool> RollBackSynchronizationAsync(int userId, string fileGroupName);
        /// <summary>
        /// Начать новую синхронизацию.
        /// </summary>
        /// <param name="userId">id пользователя, выполняющего действие</param>
        /// <param name="fileGroupName">Имя файловой группы</param>
        /// <returns></returns>
        Task<int> StartNewSynchronizationAsync(int userId, string fileGroupName);
    }
}