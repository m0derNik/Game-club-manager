using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameClubManager.Client.Pages;
using GameClubManager.Client.Services;
using GameClubManager.Client.ViewModels;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Data;

namespace GameClubManager.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button currentButton;
        private readonly TimeService _timeService;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            // Инициализируем TimeService
            _timeService = TimeService.Instance;
            
            // Создаем ViewModel
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            
            // Отображаем страницу авторизации
            AuthFrame.Navigate(new LoginPage());

            // Подписываемся на события авторизации
            AuthManager.Instance.LoggedOut += AuthManager_LoggedOut;
            AuthManager.Instance.LoggedIn += AuthManager_LoggedIn;

            // Блокируем Alt+F4 и Alt+Tab
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            
            // Обновляем данные при загрузке
            Loaded += MainWindow_Loaded;
        }
        
        private void TextBlock_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                // Мигание при обновлении
                var originalBrush = textBlock.Foreground;
                textBlock.Foreground = Brushes.LimeGreen;
                
                Trace.WriteLine($"Обновление TextBlock из источника: {textBlock.Text}, {DateTime.Now}");
                
                // Возвращаем цвет через 300 мс
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                timer.Tick += (s, args) =>
                {
                    textBlock.Foreground = originalBrush;
                    (s as System.Windows.Threading.DispatcherTimer).Stop();
                };
                timer.Start();
            }
        }
        
        private void TextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                Trace.WriteLine($"Обновление TextBlock в цели: {textBlock.Text}, {DateTime.Now}");
            }
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Обновляем данные при загрузке окна
            _viewModel.UpdateDataFromService();
            Trace.WriteLine("MainWindow загружено, данные обновлены");
        }
        
        private void AuthManager_LoggedIn(object sender, EventArgs e)
        {
            ShowMainContent();
            
            // Вызываем обновление данных
            _viewModel.UpdateDataFromService();
            Trace.WriteLine("Пользователь вошел в систему, данные обновлены");
        }
        
        private void AuthManager_LoggedOut(object sender, EventArgs e)
        {
            ShowLoginPage();
            Trace.WriteLine("Пользователь вышел из системы");
        }

        public void ShowMainContent()
        {
            // Показываем основное содержимое
            MainGrid.Visibility = Visibility.Visible;
            AuthFrame.Visibility = Visibility.Collapsed;

            // Очищаем предыдущие обработчики, чтобы избежать дублирования
            ProfileButton.Click -= ProfileButton_Click;
            ProgramsButton.Click -= ProgramsButton_Click;
            FoodButton.Click -= FoodButton_Click;
            TariffButton.Click -= TariffButton_Click;
            SettingsButton.Click -= SettingsButton_Click;
            AdminHelpButton.Click -= AdminHelpButton_Click;

            // Инициализируем кнопки навигации
            ProfileButton.Click += ProfileButton_Click;
            ProgramsButton.Click += ProgramsButton_Click;
            FoodButton.Click += FoodButton_Click;
            TariffButton.Click += TariffButton_Click;
            SettingsButton.Click += SettingsButton_Click;
            AdminHelpButton.Click += AdminHelpButton_Click;

            // Начальная страница
            NavigateToPage(ProfileButton, new ProfilePage());
            
            Trace.WriteLine("Показано основное содержимое");
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(ProfileButton, new ProfilePage());
        private void ProgramsButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(ProgramsButton, new ProgramsPage());
        private void FoodButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(FoodButton, new FoodPage());
        private void TariffButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(TariffButton, new TariffPage());
        private void SettingsButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(SettingsButton, new SettingsPage());
        private void AdminHelpButton_Click(object sender, RoutedEventArgs e) => ShowAdminHelp();

        private void NavigateToPage(Button button, Page page)
        {
            // Сбрасываем подсветку текущей кнопки
            if (currentButton != null)
            {
                currentButton.Background = (SolidColorBrush)FindResource("PrimaryBrush");
            }

            // Подсвечиваем новую кнопку
            currentButton = button;
            button.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));

            // Загружаем страницу в основной контент
            MainContent.Navigate(page);
            
            Trace.WriteLine($"Навигация на страницу: {page.GetType().Name}");
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

        private void ShowLoginPage()
        {
            // Скрываем основное содержимое
            MainGrid.Visibility = Visibility.Collapsed;
            AuthFrame.Visibility = Visibility.Visible;
            
            // Переходим на страницу авторизации
            AuthFrame.Navigate(new LoginPage());
        }
    }
}