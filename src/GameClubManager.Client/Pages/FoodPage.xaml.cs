using System.Windows.Controls;
using GameClubManager.Client.ViewModels;
using GameClubManager.Client.Views;

namespace GameClubManager.Client.Pages
{
    public partial class FoodPage : Page
    {
        private readonly FoodViewModel _viewModel;
        
        public FoodPage()
        {
            InitializeComponent();
            
            // Создаем ViewModel и устанавливаем как DataContext
            _viewModel = new FoodViewModel();
            DataContext = _viewModel;
            
            // Подписываемся на события
            _viewModel.ShowCartRequested += OnShowCartRequested;
        }
        
        private void OnShowCartRequested(object? sender, System.EventArgs e)
        {
            // Создаем диалоговое окно корзины
            var cartViewModel = new FoodCartViewModel();
            var cartDialog = new FoodCartDialog(cartViewModel);
            
            // Отображаем диалоговое окно
            cartDialog.ShowDialog();
        }
    }
} 