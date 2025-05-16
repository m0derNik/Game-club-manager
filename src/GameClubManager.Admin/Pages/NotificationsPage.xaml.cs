using System.Windows.Controls;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin.Pages
{
    public partial class NotificationsPage : Page
    {
        public NotificationsPage()
        {
            InitializeComponent();
            this.DataContext = new NotificationsViewModel();
        }
    }
} 