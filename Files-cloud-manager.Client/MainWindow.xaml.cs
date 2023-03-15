using Files_cloud_manager.Client.Configs;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using Files_cloud_manager.Client.ViewModels;
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

namespace Files_cloud_manager.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            // todo перенести.
            var services = new ServiceCollection();
            services.AddTransient<IFileHashCheckerService, FileHashCheckerService>();
            services.AddTransient<IServerConnectionService, ServerConnectionService>();
            services.AddTransient<ModelFactory>();
            services.AddSingleton<DialogServiceConfig>(e=>new DialogServiceConfig() { MainWindow=this});
            services.AddTransient<IDialogService,DialogService>();

            var serviceProvider = services.BuildServiceProvider();

            var scope = serviceProvider.CreateScope();

            var a = scope.ServiceProvider.GetRequiredService<ModelFactory>();
            var b = a.CreateProgramsListModel("admin", "123");

            b.CreateNewProgramData("", "C:\\programsM\\file\\test", "test5");

            //  var b = scope.ServiceProvider.GetService<IServerConnectionService>();
            //b.LoginAsync("admin", "123").Wait();

            this.DataContext = new ProgramListViewModel(b, scope.ServiceProvider.GetRequiredService<IDialogService>());


            //   CancellationTokenSource source = new CancellationTokenSource();

            // Console.WriteLine();

            // programDataModel.Login();

            //Console.WriteLine();
            InitializeComponent();
        }
    }
}
