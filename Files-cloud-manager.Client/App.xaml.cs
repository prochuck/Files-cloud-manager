using Files_cloud_manager.Client.Configs;
using Files_cloud_manager.Client.Services.Interfaces;
using Files_cloud_manager.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Files_cloud_manager.Client.Extentions;

using Microsoft.Extensions.Configuration;
using System.IO;
using Files_cloud_manager.Client.Commands;

namespace Files_cloud_manager.Client
{
    //todo сделать логин асинхронным.

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            IConfiguration config = AddCondfiguration();


            services.AddSingleton<ProgramListCaretakerConfig>(e => config.GetProgramListCaretakerConfig());
            services.AddTransient<IFileHashCheckerService, FileHashCheckerService>();
            services.AddTransient<IServerConnectionService, ServerConnectionService>();
            services.AddTransient<IProgramListCaretaker, ProgramListCaretaker>();
            services.AddSingleton<ModelFactory>();
            services.AddSingleton<IDialogService, DialogService>(
                e =>
                 {
                     var res = ActivatorUtilities.CreateInstance<DialogService>(e);
                     res.RegisterGroupCreationDialog();
                     res.RegisterLoginDialog();
                     return res;
                 }
                );

            services.AddSingleton<MainWindow>();
        }

        private IConfiguration AddCondfiguration()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", false);

#if DEBUG
            builder.AddJsonFile("appsettings.Development.json", true);
#endif

            return builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Window window = serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }
    }
}
