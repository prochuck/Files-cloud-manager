using FileCloudAPINameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services.Interfaces
{
    internal interface IServerConnectionService : IDisposable
    {
        public bool IsLoogedIn { get; }
        public bool IsSyncStarted { get; }
        /// <summary>
        /// Отправить файл на сервер. Если файл существует на сервере, он будет обновлён.
        /// </summary>
        /// <param name="syncId">Id синхронизации</param>
        /// <param name="filePath">путь файла, по которому он будет сохранён на сервере.</param>
        /// <param name="file">Файл.</param>
        /// <returns></returns>
        Task<bool> CreateOrUpdateFileAsync(string filePath, Stream file, CancellationToken cancellationToken);
        /// <summary>
        /// Удаление файла на сервере.
        /// </summary>
        /// <param name="filePath">путь файла на сервере.</param>
        /// <returns></returns>
        Task<bool> DeleteFileAsync(string filePath);
        /// <summary>
        /// Скачать файл с сервера
        /// </summary>   
        /// <param name="filePath">путь файла на сервере.</param>
        /// <returns></returns>
        Task<Stream> DonwloadFileAsync(string filePath, CancellationToken cancellationToken);
        /// <summary>
        /// Закончить синхронизацию и применить изменения на сервере.
        /// </summary>
        /// <returns></returns>
        Task<bool> EndSyncAsync();
        /// <summary>
        /// Получить информацию о файлах на сервере. 
        /// </summary>
        /// <returns>Список файлов на сервере. В т.ч. их пути и хэши</returns>
        Task<ICollection<FileInfoDTO>> GetFilesAsync();
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
        /// <returns></returns>
        Task<bool> RollBackSyncAsync();
        /// <summary>
        /// Начать синхронизацию файлов между клиентом и сервером
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Начата ли синхронизация</returns>
        Task<bool> StartSynchronizationAsync(string groupName);
    }
}