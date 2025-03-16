using System.Windows;
using System.Windows.Controls;

namespace GameClubManager.Client.Services
{
    public static class AuthManager
    {
        private static string _adminPassword = "admin"; // Временный пароль для демонстрации

        public static void Initialize(string adminPassword)
        {
            _adminPassword = adminPassword;
        }

        public static bool VerifyPassword()
        {
            var dialog = new PasswordDialog();
            if (dialog.ShowDialog() == true)
            {
                return dialog.Password == _adminPassword;
            }
            return false;
        }
    }

    public class PasswordDialog : Window
    {
        private PasswordBox passwordBox;
        public string Password => passwordBox.Password;

        public PasswordDialog()
        {
            Title = "Введите пароль администратора";
            Width = 300;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

            passwordBox = new PasswordBox
            {
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(passwordBox);

            var buttonPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            Grid.SetRow(buttonPanel, 1);

            var okButton = new Button
            {
                Content = "OK",
                Width = 60,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 60,
                IsCancel = true
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
} 