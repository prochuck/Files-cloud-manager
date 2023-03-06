using FileCloudAPINameSpace;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services.Interfaces
{
    internal interface IServerConnectionService
    {
        /// <summary>
        /// Отправить файл на сервер. Если файл существует на сервере, он будет обновлён.
        /// </summary>
        /// <param name="syncId">Id синхронизации</param>
        /// <param name="filePath">путь файла, по которому он будет сохранён на сервере.</param>
        /// <param name="file">Файл.</param>
        /// <returns></returns>
        Task<bool> CreateOrUpdateFileAsync(int syncId, string filePath, Stream file);
        /// <summary>
        /// Удаление файла на сервере.
        /// </summary>
        /// <param name="syncId">Id синхронизации</param>
        /// <param name="filePath">путь файла на сервере.</param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(int syncId, string filePath);
        /// <summary>
        /// Скачать файл с сервера
        /// </summary>   
        /// <param name="syncId">Id синхронизации</param>
        /// <param name="filePath">путь файла на сервере.</param>
        /// <returns></returns>
        Task<Stream> DonwloadFileAsync(int syncId, string filePath);
        /// <summary>
        /// Закончить синхронизацию и применить изменения на сервере.
        /// </summary>
        /// <param name="syncId">Id синхронизации</param>
        /// <returns></returns>
        Task<bool> EndSyncAsync(int syncId);
        /// <summary>
        /// Получить информацию о файлах на сервере. 
        /// </summary>
        /// <param name="syncId">Id синхронизации</param>
        /// <returns>Список файлов на сервере. В т.ч. их пути и хэши</returns>
        Task<ICollection<FileInfoDTO>> GetFilesAsync(int syncId);
        /// <summary>
        /// Залогиниться на сервере.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> LoginAsync(string login, string password);
        /// <summary>
        /// ВЫйти из учётки на сервере.
        /// </summary>
        /// <returns></returns>
        Task<bool> LogoutAsync();
        /// <summary>
        /// Обновить авторизацию на сервере.
        /// </summary>
        /// <returns></returns>
        Task RefreshCoockie();
       /// <summary>
       /// Закончить синхронизацию и отменить изменения.
       /// </summary>
       /// <param name="syncId"></param>
       /// <returns></returns>
        Task<bool> RollBackSyncAsync(int syncId);
        /// <summary>
        /// Начать синхронизацию файлов между клиентом и сервером
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Id синхронизации. syncId</returns>
        Task<int> StartSynchronizationAsync(string groupName);
    }
}