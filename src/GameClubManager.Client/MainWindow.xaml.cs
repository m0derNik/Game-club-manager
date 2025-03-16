using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameClubManager.Client.Pages;
using GameClubManager.Client.Services;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button currentButton;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            ProfileButton.Click += (s, e) => NavigateToPage(ProfileButton, new ProfilePage());
            ProgramsButton.Click += (s, e) => NavigateToPage(ProgramsButton, new ProgramsPage());
            FoodButton.Click += (s, e) => NavigateToPage(FoodButton, new FoodPage());
            TariffButton.Click += (s, e) => NavigateToPage(TariffButton, new TariffPage());
            SettingsButton.Click += (s, e) => NavigateToPage(SettingsButton, new SettingsPage());
            AdminHelpButton.Click += (s, e) => ShowAdminHelp();

            // Начальная страница
            NavigateToPage(ProfileButton, new ProfilePage());

            // Блокируем Alt+F4 и Alt+Tab
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void NavigateToPage(Button button, Page page)
        {
            // Сбрасываем подсветку текущей кнопки
            if (currentButton != null)
            {
                currentButton.Background = Brushes.Transparent;
            }

            // Подсвечиваем новую кнопку
            currentButton = button;
            button.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

            // Загружаем страницу
            MainContent.Navigate(page);
        }

        private void ShowAdminHelp()
        {
            MessageBox.Show("Админ в пути", "Помощь админа", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Блокируем Alt+F4 и Alt+Tab
            if (e.Key == Key.System && (e.SystemKey == Key.F4 || e.SystemKey == Key.Tab))
            {
                e.Handled = true;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Запрашиваем пароль при закрытии
            if (!AuthManager.VerifyPassword())
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Обработка перетаскивания окна
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}