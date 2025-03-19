using System.Windows.Controls;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
} 