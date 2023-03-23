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
        public MainWindow(IServiceProvider provider)
        {
            // todo вынести конфиги в отдельные файл
            // todo перенести.
            _scope = provider.CreateScope();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IDialogService thing = _scope.ServiceProvider.GetRequiredService<IDialogService>();
            ModelFactory modelFactory = _scope.ServiceProvider.GetRequiredService<ModelFactory>();
            var res = thing.ShowLoginDialog(this);
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
