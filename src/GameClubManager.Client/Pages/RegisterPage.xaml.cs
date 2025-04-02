using GameClubManager.Client.Services;
using System.Windows;
using System.Windows.Controls;

namespace GameClubManager.Client.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            
            // Обработчики событий
            RegisterButton.Click += RegisterButton_Click;
            BackToLoginButton.Click += BackToLoginButton_Click;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = NameTextBox.Text;
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await AuthManager.Instance.RegisterAsync(username, email, password);
            if (success)
            {
                MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.ShowMainContent();
                }
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
} 