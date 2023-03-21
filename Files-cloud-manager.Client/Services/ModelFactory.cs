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
    public class ModelFactory : IDisposable
    {
        private IServiceProvider _serviceProvider;
        private IServiceScope _serviceScope;

        public ModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = serviceProvider.CreateScope();
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
        public ProgramsListModel? CreateProgramsListModel(string login, string password)
        {
            var connectionService= _serviceScope.ServiceProvider.GetService<IServerConnectionService>()!;
            if (!connectionService.LoginAsync(login, password).Result)
            {
                return null;
            }
            var res = new ProgramsListModel(
                _serviceScope.ServiceProvider.GetService<ModelFactory>()!,
                connectionService);
            return res;
        }

        public void Dispose()
        {

            _serviceScope.Dispose();
        }
    }
}
