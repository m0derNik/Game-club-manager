using System.Windows.Controls;

namespace GameClubManager.Client.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            DataContext = new ViewModels.ProfileViewModel();
        }
    }
} 