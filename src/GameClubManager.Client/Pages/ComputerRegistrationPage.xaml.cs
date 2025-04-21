using System.Windows.Controls;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client.Pages
{
    public partial class ComputerRegistrationPage : Page
    {
        public ComputerRegistrationPage()
        {
            InitializeComponent();
            DataContext = new ComputerRegistrationViewModel();
        }
    }
} 