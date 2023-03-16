using FileCloudAPINameSpace;
using Files_cloud_manager.Client.ViewModels;
using System.Collections.Generic;

namespace Files_cloud_manager.Client.Services.Interfaces
{
    /// <summary>
    /// Сервис отображения диалогов
    /// </summary>
    internal interface IDialogService
    {
        /// <summary>
        /// Вызвать диалог создания файловой группы. 
        /// </summary>
        /// <param name="fileInfoGroupDTOs"></param>
        /// <returns></returns>
        GroupCreationViewModel ShowGroupCreationDialog(List<FileInfoGroupDTO> fileInfoGroupDTOs);
    }
}