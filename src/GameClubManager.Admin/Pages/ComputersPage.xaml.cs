using System.Windows.Controls;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin.Pages
{
    public partial class ComputersPage : Page
    {
        public ComputersPage()
        {
            InitializeComponent();
            DataContext = new ComputersViewModel();
        }
    }
} 