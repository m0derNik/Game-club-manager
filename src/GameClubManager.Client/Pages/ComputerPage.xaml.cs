using System.Windows.Controls;
using GameClubManager.Client.Models;
using GameClubManager.Client.UserControls;
using GameClubManager.Client.ViewModels;

namespace GameClubManager.Client.Pages
{
    public partial class ComputerPage : Page
    {
        private readonly ComputerPageViewModel _viewModel;
        
        public ComputerPage()
        {
            InitializeComponent();
            
            // Создаем ViewModel и устанавливаем как DataContext
            _viewModel = new ComputerPageViewModel();
            DataContext = _viewModel;
            
            // Подписываемся на события выбора компьютера
            computerSelector.ComputerSelected += OnComputerSelected;
            computerSelector.SelectionCancelled += OnSelectionCancelled;
        }
        
        private async void OnComputerSelected(object sender, ComputerSelectedEventArgs e)
        {
            if (e.Computer != null)
            {
                // Пытаемся привязать выбранный компьютер
                await _viewModel.BindComputerAsync(e.Computer);
            }
        }
        
        private void OnSelectionCancelled(object sender, System.EventArgs e)
        {
            // Скрываем селектор компьютеров
            _viewModel.CancelComputerSelection();
        }
    }
} 


