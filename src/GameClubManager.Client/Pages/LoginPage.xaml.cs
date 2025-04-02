using GameClubManager.Client.Services;
using System.Windows;
using System.Windows.Controls;

namespace GameClubManager.Client.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            
            // Обработчики событий
            LoginButton.Click += LoginButton_Click;
            RegisterButton.Click += RegisterButton_Click;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await AuthManager.Instance.LoginAsync(email, password);
            if (success)
            {
                // TODO: Перейти на главную страницу
                MessageBox.Show("Успешный вход!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ShowMainContent();
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Перейти на страницу регистрации
            NavigationService?.Navigate(new RegisterPage());
        }
    }
} 