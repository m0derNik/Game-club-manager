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
        private System.Windows.Controls.Button currentButton;
        private readonly TimeService _timeService;
        private readonly GameClubManager.Client.ViewModels.MainWindowViewModel _viewModel;
        private readonly ComputerRegistrationService _computerService;

        public MainWindow()
        {
            InitializeComponent();
            
            // �������������� TimeService
            _timeService = TimeService.Instance;
            
            // �������������� ComputerService
            _computerService = ComputerRegistrationService.Instance;
            
            // ������� ViewModel
            _viewModel = new GameClubManager.Client.ViewModels.MainWindowViewModel();
            DataContext = _viewModel;
            
            // ���������� �������� �����������
            AuthFrame.Navigate(new LoginPage());

            // ������������� �� ������� �����������
            AuthManager.Instance.LoggedOut += AuthManager_LoggedOut;
            AuthManager.Instance.LoggedIn += AuthManager_LoggedIn;

            // ��������� Alt+F4 � Alt+Tab
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            
            // ��������� ������ ��� ��������
            Loaded += MainWindow_Loaded;
        }
        
        private void TextBlock_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                // ������� ��� ����������
                var originalBrush = textBlock.Foreground;
                textBlock.Foreground = System.Windows.Media.Brushes.LimeGreen;
                
                Trace.WriteLine($"���������� TextBlock �� ���������: {textBlock.Text}, {DateTime.Now}");
                
                // ���������� ���� ����� 300 ��
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
                Trace.WriteLine($"���������� TextBlock � ����: {textBlock.Text}, {DateTime.Now}");
            }
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ������������ ��������� ��� ������� ����������
            await _computerService.RegisterComputerAsync();
            
            // ��������� ������ ��� �������� ����
            _viewModel.UpdateDataFromService();
            Trace.WriteLine("MainWindow ���������, ������ ���������");
        }
        
        private async void AuthManager_LoggedIn(object sender, EventArgs e)
        {
            ShowMainContent();
            
            // �������� ���������� ������
            _viewModel.UpdateDataFromService();
            
            // ��������� ������ ���������� - ����� ������� �������������
            var userId = AuthManager.Instance.CurrentUserId;
            if (userId > 0)
            {
                await _computerService.UpdateStatusOnLoginAsync(userId);
            }
            
            Trace.WriteLine("������������ ����� � �������, ������ ���������");
        }
        
        private async void AuthManager_LoggedOut(object sender, EventArgs e)
        {
            // ��������� ������ ���������� - ��������
            await _computerService.UpdateStatusOnLogoutAsync();
            
            ShowLoginPage();
            Trace.WriteLine("������������ ����� �� �������");
        }

        public void ShowMainContent()
        {
            // ���������� �������� ����������
            MainGrid.Visibility = Visibility.Visible;
            AuthFrame.Visibility = Visibility.Collapsed;

            // ������� ���������� �����������, ����� �������� ������������
            ProfileButton.Click -= ProfileButton_Click;
            ProgramsButton.Click -= ProgramsButton_Click;
            FoodButton.Click -= FoodButton_Click;
            TariffButton.Click -= TariffButton_Click;
            SettingsButton.Click -= SettingsButton_Click;
            AdminHelpButton.Click -= AdminHelpButton_Click;

            // �������������� ������ ���������
            ProfileButton.Click += ProfileButton_Click;
            ProgramsButton.Click += ProgramsButton_Click;
            FoodButton.Click += FoodButton_Click;
            TariffButton.Click += TariffButton_Click;
            SettingsButton.Click += SettingsButton_Click;
            AdminHelpButton.Click += AdminHelpButton_Click;

            // ��������� ��������
            NavigateToPage(ProfileButton, new ProfilePage());
            
            Trace.WriteLine("�������� �������� ����������");
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(ProfileButton, new ProfilePage());
        private void ProgramsButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(ProgramsButton, new ProgramsPage());
        private void FoodButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(FoodButton, new FoodPage());
        private void TariffButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(TariffButton, new TariffPage());
        private void SettingsButton_Click(object sender, RoutedEventArgs e) => NavigateToPage(SettingsButton, new SettingsPage());
        private void AdminHelpButton_Click(object sender, RoutedEventArgs e) => ShowAdminHelp();

        private void NavigateToPage(System.Windows.Controls.Button button, Page page)
        {
            // ���������� ��������� ������� ������
            if (currentButton != null)
            {
                currentButton.Background = (SolidColorBrush)FindResource("PrimaryBrush");
            }

            // ������������ ����� ������
            currentButton = button;
            button.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255));

            // ��������� �������� � �������� �������
            MainContent.Navigate(page);
            
            Trace.WriteLine($"��������� �� ��������: {page.GetType().Name}");
        }

        private async void ShowAdminHelp()
        {
            // Создаем диалоговое окно для ввода причины вызова администратора
            var dialog = new GameClubManager.Client.Dialogs.AdminHelpDialog();
            var result = dialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                string reason = dialog.Reason;
                
                // Вызываем API для отправки уведомления
                var apiService = GameClubManager.Client.Services.ApiService.Instance;
                bool success = await apiService.CallAdminAsync(reason);
                
                if (success)
                {
                    System.Windows.MessageBox.Show(
                        "Администратор был уведомлен и скоро подойдет к вам", 
                        "Вызов администратора", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // ��������� Alt+F4 � Alt+Tab
            if (e.Key == Key.System && (e.SystemKey == Key.F4 || e.SystemKey == Key.Tab))
            {
                e.Handled = true;
            }

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // ����������� ������ ��� ��������
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

        // ��������� �������������� ����
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
            // �������� �������� ����������
            MainGrid.Visibility = Visibility.Collapsed;
            AuthFrame.Visibility = Visibility.Visible;
            
            // ��������� �� �������� �����������
            AuthFrame.Navigate(new LoginPage());
        }
    }
}



