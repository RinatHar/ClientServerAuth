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
    public partial class FormWindow : Window
    {
        private readonly DataService _service = new DataService();
        private readonly UserService _userService;

        public event EventHandler DataAdded; // Событие, которое вызывается при добавлении данных

        public FormWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        private async void AddData(object sender, RoutedEventArgs e)
        {
            string value = formValue.Text;
            bool isSuccess = await _service.AddDataToServerAsync(
                "https://localhost:7218/api/data/write",
                value,
                _userService.CurrentUser.Login,
                _userService.CurrentUser.Password);
            if (isSuccess)
            {
                DataAdded?.Invoke(this, EventArgs.Empty); // Вызываем событие, если данные были успешно добавлены
            }
            this.Close();
        }
    }
}
