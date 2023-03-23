using Files_cloud_manager.Client.Configs;
using Files_cloud_manager.Client.Extentions;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using Files_cloud_manager.Client.ViewModels;
using Files_cloud_manager.Client.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace Files_cloud_manager.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IServiceScope _scope;
        public MainWindow()
        {
            // todo вынести конфиги в отдельные файл
            // todo перенести.
            var services = new ServiceCollection();
            services.AddSingleton<DialogServiceConfig>(e => new DialogServiceConfig() { MainWindow = this });
            services.AddSingleton<ProgramListCaretakerConfig>(e => new ProgramListCaretakerConfig()
            {
                PathToSaveFile = "C:\\programsM\\file\\groups.json"
            });

            services.AddTransient<IFileHashCheckerService, FileHashCheckerService>();
            services.AddTransient<IServerConnectionService, ServerConnectionService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddTransient<IProgramListCaretaker, ProgramListCaretaker>();
            services.AddSingleton<ModelFactory>();

            var _provider = services.BuildServiceProvider();

            _scope = _provider.CreateScope();

            var a = _scope.ServiceProvider.GetRequiredService<ModelFactory>();

            var thing = _scope.ServiceProvider.GetRequiredService<IDialogService>();
            thing.RegisterLoginDialog();
            thing.RegisterGroupCreationDialog();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var thing = _scope.ServiceProvider.GetRequiredService<IDialogService>();
            var res = thing.ShowLoginDialog();
            if (res is null)
            {
                this.Close();
                return;
            }

            this.DataContext = new ProgramListViewModel(
                res.ProgramsListModel,
                _scope.ServiceProvider.GetRequiredService<IProgramListCaretaker>(),
                _scope.ServiceProvider.GetRequiredService<IDialogService>());
        }
    }
}
