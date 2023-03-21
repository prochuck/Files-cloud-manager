using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.ViewModels
{
    internal class LoginViewModel : ViewModelBase
    {
        public ProgramsListModel ProgramsListModel { get; set; }

        private ModelFactory _modelFactory;

        string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public LoginViewModel(ModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
        }

        public bool Validate()
        {
            var res = _modelFactory.CreateProgramsListModel(Login, Password);

            if (res is null)
            {
                return false;
            }

            ProgramsListModel = res;

            return true;
        }

    }
}
