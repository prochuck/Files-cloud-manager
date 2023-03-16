using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Models.DTO;

namespace Files_cloud_manager.Server.Services.Interfaces
{
    /// <summary>
    /// Сервис синхронизации файов.
    /// </summary>
    public interface IFilesSynchronizationService
    {
        /// <summary>
        /// Создать или заменить существующий файл.
        /// </summary>
        /// <param name="filePath">Относительный путь файла</param>
        /// <param name="originalFileStream">Файл который будет создан/ на который будет заменён</param>
        /// <returns></returns>
        Task<bool> CreateOrUpdateFileAsync(string filePath, Stream originalFileStream);
        /// <summary>
        /// Удалить файл.
        /// </summary>
        /// <param name="filePath">Относительный путь к удаляемому файлу.</param>
        /// <returns></returns>
        bool DeleteFile(string filePath);
        /// <summary>
        /// Закончить синхронизацию и применить изменения.
        /// </summary>
        /// <returns></returns>
        bool EndSynchronization();
        /// <summary>
        /// Получить файл по относительному пути.
        /// </summary>
        /// <param name="filePath">Относительный путь к файлу</param>
        /// <returns></returns>
        Stream GetFile(string filePath);
        /// <summary>
        /// Получить список всех файлов.
        /// </summary>
        /// <returns>Список файлов</returns>
        public List<FileInfoDTO> GetFilesInfos();
        /// <summary>
        /// Начать синхронизацию.
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="fileInfoGroupName">Имя группы для синхронизации</param>
        /// <returns></returns>
        bool StartSynchronization(int userId, string fileInfoGroupName);
        /// <summary>
        /// Отменить синхронизацию и откатить изменения.
        /// </summary>
        /// <returns></returns>
        bool RollBackSynchronization();
    }
}