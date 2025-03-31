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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Здесь будет логика авторизации
            // Пока просто показываем основное окно
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ShowMainContent();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу регистрации
            NavigationService.Navigate(new RegisterPage());
        }
    }
} 