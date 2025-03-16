using System.Windows.Controls;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client.Pages
{
    public partial class ProgramsPage : Page
    {
        public ProgramsPage()
        {
            InitializeComponent();
            DataContext = new ProgramsViewModel();
        }
    }
} 