using System.Windows.Controls;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin.Pages
{
    public partial class TariffsPage : Page
    {
        public TariffsPage()
        {
            InitializeComponent();
            DataContext = new TariffsViewModel();
        }
    }
} 