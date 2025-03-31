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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Здесь будет логика регистрации
            // Пока просто показываем основное окно
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ShowMainContent();
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на страницу авторизации
            NavigationService.Navigate(new LoginPage());
        }
    }
} 