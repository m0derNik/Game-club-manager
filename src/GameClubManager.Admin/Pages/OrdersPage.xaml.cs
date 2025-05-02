using System.Windows.Controls;
using GameClubManager.Admin.ViewModels;

namespace GameClubManager.Admin.Pages
{
    public partial class OrdersPage : Page
    {
        private readonly OrdersViewModel _viewModel;
        
        public OrdersPage()
        {
            InitializeComponent();
            
            // Создаем экземпляр ViewModel
            _viewModel = new OrdersViewModel();
            
            // Устанавливаем ViewModel как DataContext
            DataContext = _viewModel;
        }
    }
} 