using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;

namespace GameClubManager.Client.UserControls
{
    public partial class ComputerSelector : UserControl, INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly ComputerBindingService _computerBindingService;
        private ObservableCollection<ComputerDto> _availableComputers;
        private bool _isLoading;
        
        // Событие, которое вызывается при выборе компьютера
        public event EventHandler<ComputerSelectedEventArgs> ComputerSelected;
        
        // Событие, которое вызывается при отмене выбора
        public event EventHandler SelectionCancelled;
        
        public ObservableCollection<ComputerDto> AvailableComputers
        {
            get => _availableComputers;
            set
            {
                _availableComputers = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand RefreshCommand { get; }
        public ICommand CancelCommand { get; }
        
        public ComputerSelector()
        {
            InitializeComponent();
            
            _apiService = ApiService.Instance;
            _computerBindingService = ComputerBindingService.Instance;
            _availableComputers = new ObservableCollection<ComputerDto>();
            
            RefreshCommand = new RelayCommand(_ => LoadComputers());
            CancelCommand = new RelayCommand(_ => OnCancelClicked());
            
            // Устанавливаем DataContext на текущий экземпляр
            DataContext = this;
            
            // Загружаем список компьютеров при инициализации
            Loaded += (sender, args) => LoadComputers();
        }
        
        private async void LoadComputers()
        {
            try
            {
                IsLoading = true;
                
                // Получаем список компьютеров с сервера
                var computers = await _apiService.GetComputersAsync();
                
                AvailableComputers.Clear();
                foreach (var computer in computers)
                {
                    AvailableComputers.Add(computer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка компьютеров: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void Computer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ComputerDto computer)
            {
                // Если компьютер занят, показываем сообщение
                if (computer.Status == ComputerStatus.Occupied)
                {
                    MessageBox.Show("Этот компьютер уже занят другим пользователем.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // Вызываем событие выбора компьютера
                ComputerSelected?.Invoke(this, new ComputerSelectedEventArgs(computer));
            }
        }
        
        private void OnCancelClicked()
        {
            SelectionCancelled?.Invoke(this, EventArgs.Empty);
        }
        
        // Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // Аргументы события выбора компьютера
    public class ComputerSelectedEventArgs : EventArgs
    {
        public ComputerDto Computer { get; }
        
        public ComputerSelectedEventArgs(ComputerDto computer)
        {
            Computer = computer;
        }
    }
} 