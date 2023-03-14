using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    internal class ProgramsListModel
    {
        public ReadOnlyObservableCollection<ProgramDataModel> ProgramsList { get; private set; }
        public ReadOnlyObservableCollection<FileInfoGroupDTO> FileGroups { get; private set; }

        private ObservableCollection<ProgramDataModel> _programsList;
        private ObservableCollection<FileInfoGroupDTO> _fileGroups;
        private ModelFactory _ModelFactory;
        private string _login;
        private string _password;

        private IServerConnectionService _connectionService;

        public ProgramsListModel(string login, string password, ModelFactory ModelFactory, IServerConnectionService serverConnectionService)
        {
            _ModelFactory = ModelFactory;
            _login = login;
            _password = password;

            // todo Загрузку из файла списка программ.
            _programsList = new ObservableCollection<ProgramDataModel>();
            ProgramsList = new ReadOnlyObservableCollection<ProgramDataModel>(_programsList);



            if (!serverConnectionService.LoginAsync(login, password).Result)
            {
                throw new Exception("Wrong login/password");
            }
            _connectionService = serverConnectionService;


            _fileGroups = new ObservableCollection<FileInfoGroupDTO>(_connectionService.GetFileInfoGroupsAsync().Result.ToList());
            FileGroups = new ReadOnlyObservableCollection<FileInfoGroupDTO>(_fileGroups);

        }
        // todo сделать проверку путей
        public ProgramDataModel CreateNewProgramData(string PathToExe, string PathToData, string GroupName)
        {
            UpdateFileGroups();
            if (!Path.IsPathRooted(PathToData))
            {
                throw new Exception("Указан не верный путь к данным");
            }
            if (!Path.IsPathRooted(PathToExe))
            {
                throw new Exception("Указан не верный путь к exe");
            }
            if (!_fileGroups.Any(e => e.Name == GroupName))
            {
                if (!_connectionService.CreateFileGroupAsync(GroupName).Result)
                {
                    throw new Exception($"Ошибка при создании группы с именем {GroupName}, повторите попытку позже");
                }
            }
            var model = _ModelFactory.CreateProgramDataModel(_connectionService, PathToExe, PathToData, GroupName);
            _programsList.Add(model);
            return model;
        }

        public void UpdateFileGroups()
        {
            _fileGroups.Clear();
            foreach (var item in _connectionService.GetFileInfoGroupsAsync().Result)
            {
                _fileGroups.Add(item);
            }
        }

    }
}
