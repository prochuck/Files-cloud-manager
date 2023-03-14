using FileCloudAPINameSpace;
using Files_cloud_manager.Client.ViewModels;
using System.Collections.Generic;

namespace Files_cloud_manager.Client.Services
{
    internal interface IDialogService
    {
        GroupCreationViewModel ShowGroupCreationDialog(List<FileInfoGroupDTO> fileInfoGroupDTOs);
    }
}