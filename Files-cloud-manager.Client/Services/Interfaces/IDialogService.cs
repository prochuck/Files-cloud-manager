﻿using FileCloudAPINameSpace;
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
        T ShowDialog<T>(Window? ownerWindow, params object[] parameters) where T : ViewModelBase;
    }
}