using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;
using GameClubManager.Client.Commands;
using GameClubManager.Shared.Models;

namespace GameClubManager.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<Computer> _computers;
        public ObservableCollection<Computer> Computers
        {
            get => _computers;
            set
            {
                _computers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Game> _games;
        public ObservableCollection<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged();
            }
        }

        private bool _isAdminPopupOpen;
        public bool IsAdminPopupOpen
        {
            get => _isAdminPopupOpen;
            set
            {
                _isAdminPopupOpen = value;
                OnPropertyChanged();
            }
        }

        private string _title = "Game Club Manager";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public ICommand ShowComputersCommand { get; }
        public ICommand ShowBookingsCommand { get; }
        public ICommand ShowGamesCommand { get; }
        public ICommand ShowProfileCommand { get; }
        public ICommand BookComputerCommand { get; }
        public ICommand LaunchGameCommand { get; }
        public ICommand ShowAdminPanelCommand { get; }
        public ICommand ConfirmAdminPasswordCommand { get; }

        private readonly string _configFilePath = "config.json";
        private GameConfig _gameConfig;

        public string RemainingTime => TimeService.Instance.RemainingTime;
        public decimal Balance => TimeService.Instance.Balance;

        public MainViewModel()
        {
            // Инициализация команд
            ShowComputersCommand = new RelayCommand(ShowComputers);
            ShowBookingsCommand = new RelayCommand(ShowBookings);
            ShowGamesCommand = new RelayCommand(ShowGames);
            ShowProfileCommand = new RelayCommand(ShowProfile);
            BookComputerCommand = new RelayCommand<Computer>(BookComputer);
            LaunchGameCommand = new RelayCommand<Game>(LaunchGame);
            ShowAdminPanelCommand = new RelayCommand(ShowAdminPanel);
            ConfirmAdminPasswordCommand = new RelayCommand<string>(ConfirmAdminPassword);

            // Загрузка тестовых данных
            LoadTestData();
            LoadConfig();

            // Подписываемся на изменения времени
            TimeService.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeService.RemainingTime))
                    OnPropertyChanged(nameof(RemainingTime));
                else if (e.PropertyName == nameof(TimeService.Balance))
                    OnPropertyChanged(nameof(Balance));
            };
        }

        private void LoadTestData()
        {
            Computers = new ObservableCollection<Computer>
            {
                new Computer
                {
                    Id = 1,
                    Name = "PC Gamer Pro",
                    Specifications = "RTX 4080, i9-13900K, 32GB RAM",
                    IsAvailable = true,
                    PricePerHour = 200,
                    Status = ComputerStatus.Available
                },
                new Computer
                {
                    Id = 2,
                    Name = "PC Gamer Ultra",
                    Specifications = "RTX 4090, i9-14900K, 64GB RAM",
                    IsAvailable = true,
                    PricePerHour = 300,
                    Status = ComputerStatus.Available
                },
                new Computer
                {
                    Id = 3,
                    Name = "PC Gamer Standard",
                    Specifications = "RTX 3060, i5-13600K, 16GB RAM",
                    IsAvailable = false,
                    PricePerHour = 150,
                    Status = ComputerStatus.InUse
                }
            };
        }

        private void LoadConfig()
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                _gameConfig = JsonSerializer.Deserialize<GameConfig>(json);
                Games = new ObservableCollection<Game>(_gameConfig.Games);
            }
            else
            {
                // Обработка отсутствия файла конфигурации
                Games = new ObservableCollection<Game>();
            }
        }

        private void ShowComputers(object parameter)
        {
            // Реализация будет добавлена позже
        }

        private void ShowBookings(object parameter)
        {
            // Реализация будет добавлена позже
        }

        private void ShowGames(object parameter)
        {
            // Реализация будет добавлена позже
        }

        private void ShowProfile(object parameter)
        {
            // Реализация будет добавлена позже
        }

        private void BookComputer(Computer computer)
        {
            if (computer != null && computer.IsAvailable)
            {
                // Здесь будет логика бронирования
            }
        }

        private void LaunchGame(Game game)
        {
            if (game != null && game.IsEnabled)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = game.ExecutablePath,
                    Arguments = game.Arguments,
                    WorkingDirectory = game.WorkingDirectory,
                    UseShellExecute = false
                };
                Process.Start(startInfo);
            }
        }

        private void ShowAdminPanel(object parameter)
        {
            IsAdminPopupOpen = true;
        }

        private void ConfirmAdminPassword(string password)
        {
            if (password == _gameConfig.AdminPassword)
            {
                // Логика выхода из приложения или открытия панели администратора
                IsAdminPopupOpen = false;
            }
            else
            {
                // Обработка неверного пароля
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 