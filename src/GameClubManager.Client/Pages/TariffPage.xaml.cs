using System.Windows.Controls;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client.Pages
{
    /// <summary>
    /// Логика взаимодействия для TariffPage.xaml
    /// </summary>
    public partial class TariffPage : Page
    {
        public TariffPage()
        {
            InitializeComponent();
            DataContext = new TariffViewModel();
        }
    }
} 