using ClientAuth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace ClientAuth
{
    /// <summary>
    /// Логика взаимодействия для FormWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService = new();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void Register(object sender, RoutedEventArgs e)
        {

            // Получаем логин и пароль из нового окна
            string login = loginValue.Text;
            string password = passValue.Text;

            // Проверка на null или пустую строку логина и пароля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми.");
                return;
            }

            try
            {
                await _authService.AddUser(login, password);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



    }
}
