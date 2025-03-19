using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Подписываемся на события кнопок
        ComputersButton.Click += ComputersButton_Click;
        OrdersButton.Click += OrdersButton_Click;
        UsersButton.Click += UsersButton_Click;
        NotificationsButton.Click += NotificationsButton_Click;
        SettingsButton.Click += SettingsButton_Click;
        LogoutButton.Click += LogoutButton_Click;

        // Загружаем начальную страницу
        NavigateToPage(new Pages.ComputersPage());
    }

    private void ComputersButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(new Pages.ComputersPage());
        _viewModel.CurrentPageTitle = "Компьютеры";
    }

    private void OrdersButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(new Pages.OrdersPage());
        _viewModel.CurrentPageTitle = "Заказы";
    }

    private void UsersButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(new Pages.UsersPage());
        _viewModel.CurrentPageTitle = "Пользователи";
    }

    private void NotificationsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(new Pages.NotificationsPage());
        _viewModel.CurrentPageTitle = "Уведомления";
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage(new Pages.SettingsPage());
        _viewModel.CurrentPageTitle = "Настройки";
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Вы уверены, что хотите выйти?",
            "Подтверждение выхода",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Close();
        }
    }

    private void NavigateToPage(Page page)
    {
        MainContent.Navigate(page);
    }

    private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
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
}