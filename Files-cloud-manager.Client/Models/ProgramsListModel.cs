using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Models.DTO;
using Files_cloud_manager.Client.Models.States;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
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
        public ProgramDataModel CreateProgramData(string PathToExe, string PathToData, string GroupName)
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
            lock (_programsList)
            {
                _programsList.Add(model);
            }
            return model;
        }
        public void UpdateFileGroups()
        {
            lock (_fileGroups)
            {
                _fileGroups.Clear();
                foreach (var item in _connectionService.GetFileInfoGroupsAsync().Result)
                {
                    _fileGroups.Add(item);
                }
            }
        }

        public void SetMemento(ProgramListMemento memento)
        {
            if (memento is null)
            {
                return;
            }
            UpdateFileGroups();
            lock (_fileGroups)
            {
                var a = Enumerable.Except(memento.ProgramsList.Select(e => e.GroupName), _fileGroups.Select(e => e.Name));
                if (a.Count() != 0)
                {
                    throw new Exception($"Группы с именами: {string.Join(" ", a)} - не существуют на сервере");
                }
            }

            lock (_programsList)
            {
                _programsList.Clear();
                foreach (var item in memento.ProgramsList)
                {
                    var dataModel = _ModelFactory.CreateProgramDataModel(_connectionService, item.PathToExe, item.PathToData, item.GroupName);
                    _programsList.Add(dataModel);
                }
            }
        }

        public ProgramListMemento CreateMemento()
        {
            ProgramListMemento memento = new ProgramListMemento();
            lock (_programsList)
            {
                foreach (var item in _programsList)
                {
                    memento.ProgramsList.Add(new ProgramDataModelDTO(item));
                }
            }
            return memento;
        }

    }
}
