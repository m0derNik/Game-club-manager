using GameClubManager.Client.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            
            // Очищаем ошибку при вводе текста
            EmailTextBox.TextChanged += (s, e) => HideError();
            PasswordBox.PasswordChanged += (s, e) => HideError();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем сообщение об ошибке
            HideError();
            
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "Вход...";
            
            try
            {
                var result = await AuthManager.Instance.LoginAsync(email, password);
                if (result.Success)
                {
                    if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.ShowMainContent();
                    }
                }
                else
                {
                    ShowError(result.ErrorMessage);
                    
                    // Подсвечиваем поля при неверных данных
                    if (result.ErrorMessage == "Неверно введенные данные")
                    {
                        HighlightErrorFields();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Произошла ошибка: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "Войти";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }
        
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
        
        private void HideError()
        {
            ErrorTextBlock.Text = string.Empty;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            
            // Убираем подсветку ошибок 
            EmailTextBox.ClearValue(Border.BorderBrushProperty);
            PasswordBox.ClearValue(Border.BorderBrushProperty);
        }
        
        private void HighlightErrorFields()
        {
            // Подсвечиваем поля красным цветом
            var errorBrush = new SolidColorBrush(Colors.Red);
            EmailTextBox.BorderBrush = errorBrush;
            PasswordBox.BorderBrush = errorBrush;
        }
    }
} 


