using FileCloudAPINameSpace;
using Files_cloud_manager.Client.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace Files_cloud_manager.Client.Services.Interfaces
{
    /// <summary>
    /// Сервис отображения диалогов
    /// </summary>
    internal interface IDialogService
    {
        void RegisterDialog<TViewModel, TView>()
            where TView : Window
            where TViewModel : ViewModelBase;
        T ShowDialog<T>(params object[] parameters) where T : ViewModelBase;

        /// <summary>
        /// Вызвать диалог создания файловой группы. 
        /// </summary>
        /// <param name="fileInfoGroupDTOs"></param>
        /// <returns></returns>
        GroupCreationViewModel ShowGroupCreationDialog(List<FileInfoGroupDTO> fileInfoGroupDTOs);
        LoginViewModel ShowLoginDialog();
    }
}