using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using Files_cloud_manager.Client.ViewModels;
using Files_cloud_manager.Client.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Extentions
{
    internal static class DialogServiceExtentions
    {
        public static void RegisterGroupCreationDialog(this IDialogService service)
        {
            service.RegisterDialog<GroupCreationViewModel, GroupCreationView>();
        }
        public static void RegisterLoginDialog(this IDialogService service)
        {
            service.RegisterDialog<LoginViewModel, LoginView>();
        }

        public static GroupCreationViewModel ShowGroupCreationDialog(this IDialogService service, List<FileInfoGroupDTO> fileInfoGroupDTOs)
        {      
            return service.ShowDialog<GroupCreationViewModel>(fileInfoGroupDTOs);
        }      
        public static LoginViewModel ShowLoginDialog(this IDialogService service)
        {
            return service.ShowDialog<LoginViewModel>();
        }
    }
}
