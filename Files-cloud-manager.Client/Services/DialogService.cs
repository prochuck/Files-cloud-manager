﻿using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Configs;
using Files_cloud_manager.Client.Services.Interfaces;
using Files_cloud_manager.Client.ViewModels;
using Files_cloud_manager.Client.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Files_cloud_manager.Client.Services
{
    internal class DialogService : IDialogService
    {
        private Window _mainWindow;

        private Dictionary<Type, Type> viewViewModelPairs = new Dictionary<Type, Type>();

        public DialogService(DialogServiceConfig config)
        {
            _mainWindow = config.MainWindow;
        }

        public T ShowDialog<T>(object? param)
        {
            
        }

        public GroupCreationViewModel ShowGroupCreationDialog(List<FileInfoGroupDTO> fileInfoGroupDTOs)
        {
            GroupCreationView groupCreationView = new GroupCreationView(fileInfoGroupDTOs);
            groupCreationView.Owner = _mainWindow;
            if (groupCreationView.ShowDialog() is bool res && res)
            {
                return groupCreationView.DataContext as GroupCreationViewModel;
            }
            else
            {
                return null;
            }
        }

    }
}
