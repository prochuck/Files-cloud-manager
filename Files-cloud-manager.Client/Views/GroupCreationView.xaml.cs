using FileCloudAPINameSpace;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Files_cloud_manager.Client.Views
{
    /// <summary>
    /// Interaction logic for GroupCreationView.xaml
    /// </summary>
    public partial class GroupCreationView : Window
    {
        public GroupCreationView(List<FileInfoGroupDTO> fileInfoGroups)
        {
           
            InitializeComponent();

            this.DataContext=new GroupCreationViewModel(fileInfoGroups);
        }


        // todo переместить в ViewModel
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if ((DataContext as GroupCreationViewModel)?.Validate() is bool res && res)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(this,"Заполните все поля");
            }
        }
    }
}
