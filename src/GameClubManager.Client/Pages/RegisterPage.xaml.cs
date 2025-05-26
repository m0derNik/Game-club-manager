using GameClubManager.Client.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            
            // Очищаем ошибку при вводе текста
            NameTextBox.TextChanged += (s, e) => HideError();
            EmailTextBox.TextChanged += (s, e) => HideError();
            PasswordBox.PasswordChanged += (s, e) => HideError();
            ConfirmPasswordBox.PasswordChanged += (s, e) => HideError();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем сообщение об ошибке
            HideError();
            
            var username = NameTextBox.Text;
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            // Валидация полей с подсветкой проблемного поля
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Пожалуйста, введите имя пользователя");
                HighlightErrorField(NameTextBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Пожалуйста, введите email");
                HighlightErrorField(EmailTextBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Пожалуйста, введите пароль");
                HighlightErrorField(PasswordBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Пожалуйста, подтвердите пароль");
                HighlightErrorField(ConfirmPasswordBox);
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                HighlightErrorField(PasswordBox);
                HighlightErrorField(ConfirmPasswordBox);
                return;
            }
            
            if (password.Length < 6)
            {
                ShowError("Пароль должен содержать не менее 6 символов");
                HighlightErrorField(PasswordBox);
                return;
            }

            // Проверка формата email
            if (!IsValidEmail(email))
            {
                ShowError("Указан некорректный email");
                HighlightErrorField(EmailTextBox);
                return;
            }

            RegisterButton.IsEnabled = false;
            RegisterButton.Content = "Регистрация...";
            
            try
            {
                var result = await AuthManager.Instance.RegisterAsync(username, email, password);
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
                    
                    // Подсвечиваем нужные поля в зависимости от ошибки
                    if (result.ErrorMessage.Contains("email"))
                    {
                        HighlightErrorField(EmailTextBox);
                    }
                    else if (result.ErrorMessage.Contains("имя пользователя") || result.ErrorMessage.Contains("Имя пользователя"))
                    {
                        HighlightErrorField(NameTextBox);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Произошла ошибка: {ex.Message}");
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                RegisterButton.Content = "Зарегистрироваться";
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
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
            
            // Убираем подсветку всех полей
            NameTextBox.ClearValue(Border.BorderBrushProperty);
            EmailTextBox.ClearValue(Border.BorderBrushProperty);
            PasswordBox.ClearValue(Border.BorderBrushProperty);
            ConfirmPasswordBox.ClearValue(Border.BorderBrushProperty);
        }
        
        private void HighlightErrorField(System.Windows.Controls.Control field)
        {
            field.BorderBrush = new SolidColorBrush(Colors.Red);
        }
        
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
} 


