using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            ServerConnectionService programDataModel = new ServerConnectionService();

            // todo перенести.
            var services = new ServiceCollection();
            services.AddTransient<IFileHashCheckerService,FileHashCheckerService>();
            services.AddScoped<IServerConnectionService,ServerConnectionService>();
            services.AddSingleton<ProgramDataModelFactory>();

            var serviceProvider=services.BuildServiceProvider();

            var scope=serviceProvider.CreateScope();

            var a=scope.ServiceProvider.GetRequiredService<ProgramDataModelFactory>();

            a.CreateProgramDataModel("123", "123");

            Console.WriteLine();

           // programDataModel.Login();
        
            //Console.WriteLine();
            InitializeComponent();
        }
    }
}
