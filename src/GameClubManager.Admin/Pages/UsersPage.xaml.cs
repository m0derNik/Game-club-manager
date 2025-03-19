using System.Windows.Controls;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin.Pages
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            DataContext = new UsersViewModel();
        }
    }
} 