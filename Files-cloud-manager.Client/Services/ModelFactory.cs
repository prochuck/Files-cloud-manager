using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services
{
    internal class ModelFactory : IDisposable
    {
        private IServiceProvider _serviceProvider;
        private IServiceScope _serviceScope;

        public ModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = serviceProvider.CreateScope();
        }

        public SessionModel CreateSessionModel(string login,string password)
        {
            var session = new SessionModel(login, password, this);
            return session;
        }

        public ProgramDataModel CreateProgramDataModel(IServerConnectionService connectionService,string PathToExe, string PathToData, string GroupName)
        {
            var res = new ProgramDataModel(
                connectionService,
                _serviceScope.ServiceProvider.GetRequiredService<IFileHashCheckerService>(),
                PathToExe,
                PathToData,
                GroupName);
            return res;
        }
        public ProgramsListModel CreateProgramsListModel(SessionModel session)
        {
            var res = new ProgramsListModel(this, session);

            return res;
        }


        public IServerConnectionService InitializeServerConnectionService(string login, string password)
        {
            var connectionService = _serviceScope.ServiceProvider.GetRequiredService<IServerConnectionService>();
            if (!connectionService.IsLoogedIn)
            {
                bool isLoginSuc = connectionService.LoginAsync(login, password).Result;
                if (!isLoginSuc)
                {
                    return null;
                }
            }
            else
            {
                connectionService.RefreshCoockie();
            }
            return connectionService;
        }

        public void Dispose()
        {

            _serviceScope.Dispose();
        }
    }
}
