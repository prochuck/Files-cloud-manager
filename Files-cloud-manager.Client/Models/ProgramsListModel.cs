using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    internal class ProgramsListModel
    {
        private ObservableCollection<ProgramDataModel> _programsList;
        public ReadOnlyObservableCollection<ProgramDataModel> ProgramsList;
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
        }
        // todo сделать проверку путей
        public void CreateNewProgramData(string PathToExe, string PathToData, string GroupName)
        {
            if (_programsList.Any(e => e.GroupName == GroupName))
            {
                return;
            }
            _programsList.Add(_ModelFactory.CreateProgramDataModel(_connectionService, PathToExe, PathToData, GroupName));
        }
    }
}
