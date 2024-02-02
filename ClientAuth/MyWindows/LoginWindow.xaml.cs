using ClientAuth.Models;
using ClientAuth.Services;
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

namespace ClientAuth
{
    /// <summary>
    /// Логика взаимодействия для FormWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new();
        private readonly UserService _userService = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void Login(object sender, RoutedEventArgs e)
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
                List<string> perms = await _authService.LoginUser(login, password);
                MessageBox.Show("Вход выполнен.");

                string hashedPass = AuthService.GetSHA1Hash(password);

                _userService.CurrentUser = new User
                {
                    Login = login,
                    Password = hashedPass,
                    Permissions = perms
                };

                MainWindow mainWindow = new(_userService);
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void OpenRegisterWindow(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new();
            registerWindow.Show();
        }


    }
}
