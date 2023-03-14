using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Files_cloud_manager.Client.ViewModels
{
    internal class GroupCreationViewModel : ViewModelBase
    {
        public ICommand SelectDataPathCommand { get; private set; }
        public ICommand SelectExePathCommand { get; private set; }
        public ICommand OkCommand { get; private set; }



        string _pathToData;
        public string PathToData
        {
            get { return _pathToData; }
            set
            {
                _pathToData = value;
                OnPropertyChanged(nameof(PathToData));
            }
        }


        string _pathToExe;
        public string PathToExe
        {
            get { return _pathToExe; }
            set
            {
                _pathToExe = value;
                OnPropertyChanged(nameof(PathToExe));
            }
        }


        string _groupName = null;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                OnPropertyChanged(nameof(GroupName));
            }
        }


        bool _isFromExistingGroupNames;
        public bool IsFromExistingGroupNames
        {
            get { return _isFromExistingGroupNames; }
            set
            {
                _isFromExistingGroupNames = value;
                GroupName = null;
                OnPropertyChanged(nameof(IsFromExistingGroupNames));
                OnPropertyChanged(nameof(IsNotFromExistingGroupNames));
            }
        }
        public bool IsNotFromExistingGroupNames
        {
            get { return !_isFromExistingGroupNames; }
        }
        private List<FileInfoGroupDTO> _fileInfoGroups;

        public ObservableCollection<FileInfoGroupDTO> FileInfoGroups { get; private set; }

        public GroupCreationViewModel(List<FileInfoGroupDTO> fileInfoGroups)
        {
            _fileInfoGroups = fileInfoGroups;
            FileInfoGroups = new ObservableCollection<FileInfoGroupDTO>(_fileInfoGroups);
            SelectExePathCommand = new Command(e =>
            {
                var path = SelectDirectoryDialog();
                if (path is not null)
                {
                    PathToExe = path;
                }
            }, null);
            SelectDataPathCommand = new Command(e =>
            {
                var path = SelectDirectoryDialog();
                if (path is not null)
                {
                    PathToData = path;
                }
            }, null);
        }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(_groupName) || string.IsNullOrEmpty(_pathToExe) || string.IsNullOrEmpty(_pathToData))
            {
                return false;
            }
            if (!Path.IsPathRooted(_pathToExe) || !Path.IsPathRooted(_pathToData))
            {
                return false;
            }
            return true;
        }

        private string SelectDirectoryDialog()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            try
            {
                bool isSuccess = folderBrowserDialog.ShowDialog() == DialogResult.OK;
                if (isSuccess)
                {
                    return folderBrowserDialog.SelectedPath;
                }
                return null;
            }
            finally
            {
                folderBrowserDialog.Dispose();
            }
        }

    }
}
