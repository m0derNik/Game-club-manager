using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;

namespace GameClubManager.Admin.ViewModels
{
    public class ComputersViewModel : INotifyPropertyChanged
    {
        private readonly ComputerService _computerService;
        private ObservableCollection<Computer> _computers;
        private string _searchText;
        private Computer _selectedComputer;
        private bool _isLoading;
        private bool _isEmptyVisible;
        private bool _isDataVisible = true;
        private DispatcherTimer _refreshTimer;

        public ComputersViewModel()
        {
            _computerService = ComputerService.Instance;
            
            // Инициализируем коллекции
            Computers = new ObservableCollection<Computer>();
            
            // Инициализируем команды
            RestartCommand = new RelayCommand<Computer>(ExecuteRestart);
            ShutdownCommand = new RelayCommand<Computer>(ExecuteShutdown);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            ConnectCommand = new RelayCommand<Computer>(ExecuteConnect);
            
            // Загружаем компьютеры
            _ = LoadComputersAsync();
            
            // Настраиваем таймер автоматического обновления (каждые 30 секунд)
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _refreshTimer.Tick += async (s, e) => await LoadComputersAsync();
            _refreshTimer.Start();
        }

        public ObservableCollection<Computer> Computers
        {
            get => _computers;
            set
            {
                _computers = value;
                OnPropertyChanged();
                UpdateVisibility();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterComputers();
            }
        }

        public Computer SelectedComputer
        {
            get => _selectedComputer;
            set
            {
                _selectedComputer = value;
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
                UpdateVisibility();
            }
        }

        public bool IsEmptyVisible
        {
            get => _isEmptyVisible;
            set
            {
                _isEmptyVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsDataVisible
        {
            get => _isDataVisible;
            set
            {
                _isDataVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand RestartCommand { get; }
        public ICommand ShutdownCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ConnectCommand { get; }

        private async Task LoadComputersAsync()
        {
            try
            {
                IsLoading = true;
                
                var computers = await _computerService.GetAllComputersAsync();
                
                Computers.Clear();
                foreach (var computer in computers)
                {
                    Computers.Add(computer);
                }
                
                UpdateVisibility();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки компьютеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void UpdateVisibility()
        {
            IsEmptyVisible = !IsLoading && (Computers == null || Computers.Count == 0);
            IsDataVisible = !IsLoading && !IsEmptyVisible;
        }

        private void FilterComputers()
        {
            // Логика фильтрации компьютеров будет добавлена позже
        }

        private async void ExecuteRestart(Computer computer)
        {
            if (computer == null) return;
            
            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите перезапустить компьютер {computer.Name}?", 
                "Подтверждение", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                await _computerService.RestartComputerAsync(computer.Id);
            }
        }

        private async void ExecuteShutdown(Computer computer)
        {
            if (computer == null) return;
            
            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите выключить компьютер {computer.Name}?", 
                "Подтверждение", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                await _computerService.ShutdownComputerAsync(computer.Id);
            }
        }
        
        private void ExecuteRefresh()
        {
            _ = LoadComputersAsync();
        }

        private void ExecuteConnect(Computer computer)
        {
            if (computer == null) return;
            
            try
            {
                // Создаем и открываем окно удаленного управления
                var remoteDesktopWindow = new Pages.RemoteDesktopWindow(computer.Id, computer.Name);
                remoteDesktopWindow.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при подключении к удаленному рабочему столу: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 