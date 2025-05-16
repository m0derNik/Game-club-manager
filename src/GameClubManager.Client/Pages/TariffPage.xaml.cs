using System.Windows.Controls;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client.Pages
{
    /// <summary>
    /// Логика взаимодействия для TariffPage.xaml
    /// </summary>
    public partial class TariffPage : Page
    {
        private TariffViewModel _viewModel;

        public TariffPage()
        {
            InitializeComponent();
            _viewModel = new TariffViewModel();
            DataContext = _viewModel;
            
            // Подписываемся на события
            Loaded += TariffPage_Loaded;
            Unloaded += TariffPage_Unloaded;
        }

        private async void TariffPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Обновляем тарифы при загрузке страницы
            System.Console.WriteLine("Страница тарифов загружена, обновляем тарифы...");
            await _viewModel.RefreshTariffsAsync();
        }

        private void TariffPage_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Отписываемся от событий
            Loaded -= TariffPage_Loaded;
            Unloaded -= TariffPage_Unloaded;
        }
    }
} 


