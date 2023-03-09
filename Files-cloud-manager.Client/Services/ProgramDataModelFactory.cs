﻿using Files_cloud_manager.Client.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Services
{
    internal class ProgramDataModelFactory : IDisposable
    {
        private IServiceProvider _serviceProvider;
        private IServiceScope _serviceScope;

        public ProgramDataModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceScope = serviceProvider.CreateScope();
        }

        public ProgramDataModel CreateProgramDataModel(string login, string password)
        {
            var connectionService = InitializeServerConnectionService(login, password);
            if (connectionService is null)
            {
                return null;
            }

            var res = new ProgramDataModel(connectionService, _serviceScope.ServiceProvider.GetRequiredService<FileHashCheckerService>());

            return res;
        }
        public ProgramsListModel CreateProgramsListModel(string login, string password)
        {
            var connectionService = InitializeServerConnectionService(login, password);
            if (connectionService is null)
            {
                return null;
            }

            var res = new ProgramsListModel(connectionService);

            return res;
        }

        private ServerConnectionService InitializeServerConnectionService(string login, string password)
        {
            var connectionService = _serviceScope.ServiceProvider.GetRequiredService<ServerConnectionService>();
            if (!connectionService.IsLoogedIn)
            {
                bool isLoginSuc = connectionService.LoginAsync(login, password).Result;
                if (!isLoginSuc)
                {
                    return null;
                }
                connectionService.IsLoogedIn = true;
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
