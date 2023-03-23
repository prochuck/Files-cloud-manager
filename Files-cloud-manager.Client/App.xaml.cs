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
using static System.Formats.Asn1.AsnWriter;

namespace Files_cloud_manager.Client
{
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

        private void ConfigureServices(ServiceCollection services)
        {
            
            services.AddSingleton<ProgramListCaretakerConfig>(e => new ProgramListCaretakerConfig()
            {
            });

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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Window window = serviceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }
    }
}
