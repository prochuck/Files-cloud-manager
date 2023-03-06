using Files_cloud_manager.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services
{
    internal class ProgramDataModelFactory
    {
        IServiceProvider _serviceProvider;

        public ProgramDataModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ProgramDataModel CreateProgramDataModel(string login, string password)
        {
            var connectionService = _serviceProvider.GetRequiredService<ServerConnectionService>();
            bool isLoginSuc = connectionService.LoginAsync(login, password).Result;

            if (!isLoginSuc)
            {
                return null;
            }

            var res = new ProgramDataModel(connectionService, _serviceProvider.GetRequiredService<FileHashCheckerService>());

            return res;
        }
    }
}
