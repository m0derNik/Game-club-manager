using System.Windows;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для FoodCartDialog.xaml
    /// </summary>
    public partial class FoodCartDialog : Window
    {
        private readonly FoodCartViewModel _viewModel;
        
        public FoodCartDialog(FoodCartViewModel viewModel)
        {
            InitializeComponent();
            
            _viewModel = viewModel;
            
            // Устанавливаем ViewModel в качестве DataContext
            DataContext = _viewModel;
            
            // Подписываемся на команду закрытия
            _viewModel.CloseRequested += (sender, args) => Close();
            
            // Устанавливаем результат диалога
            _viewModel.PurchaseCompleted += (sender, success) => DialogResult = success;
        }
    }
} 