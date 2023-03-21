using Files_cloud_manager.Client.Services;
using Files_cloud_manager.Client.ViewModels;
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
using System.Windows.Shapes;

namespace Files_cloud_manager.Client.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView(ModelFactory modelFactory)
        {
            InitializeComponent();

            this.DataContext = new LoginViewModel(modelFactory);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((DataContext as LoginViewModel)?.Validate() is bool res && res)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(this, "Логин или пароль не верен");
            }
        }

        private void textBox_Copy_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (this.DataContext as LoginViewModel).Password = (sender as PasswordBox).Password;
        }

       
    }
}
