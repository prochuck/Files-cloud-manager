using FileCloudAPINameSpace;
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
        private IServiceProvider _serviceProvider;
        private Window _mainWindow;

        private Dictionary<Type, Type> _viewModelViewPairs = new Dictionary<Type, Type>();

        public DialogService(DialogServiceConfig config, IServiceProvider serviceProvider)
        {
            _mainWindow = config.MainWindow;
            _serviceProvider = serviceProvider;
        }

        public void RegisterDialog<TViewModel, TView>() where TView : Window where TViewModel : ViewModelBase
        {
            _viewModelViewPairs.Add(typeof(TViewModel), typeof(TView));
        }

        public T ShowDialog<T>(params object[] parameters) where T : ViewModelBase
        {
            Window view;

            view = (Window)ActivatorUtilities.CreateInstance(_serviceProvider, _viewModelViewPairs[typeof(T)], parameters);

            view.Owner = _mainWindow;
            if (view.ShowDialog() is bool res && res)
            {
                if (view.DataContext is not T)
                {
                    throw new Exception($"DataContext view не является типом {nameof(T)}");
                }
                return ((T)view.DataContext);
            }
            else
            {
                return null;
            }
        }

        public GroupCreationViewModel ShowGroupCreationDialog(List<FileInfoGroupDTO> fileInfoGroupDTOs)
        {
            return ShowDialog<GroupCreationViewModel>(fileInfoGroupDTOs);
        }
        public LoginViewModel ShowLoginDialog()
        {
            return ShowDialog<LoginViewModel>();
        }
    }
}
