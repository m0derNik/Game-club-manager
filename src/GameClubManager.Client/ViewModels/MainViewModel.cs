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
using System.Threading.Tasks;
using SharedStatus = GameClubManager.Shared.Models.ComputerStatus;

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

        private ObservableCollection<Client.Models.Game> _games;
        public ObservableCollection<Client.Models.Game> Games
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

        private readonly AuthManager _authManager;
        private readonly TimeService _timeService;

        public MainViewModel()
        {
            _authManager = AuthManager.Instance;
            _timeService = TimeService.Instance;

            // Инициализация команд
            ShowComputersCommand = new RelayCommand(ShowComputers);
            ShowBookingsCommand = new RelayCommand(ShowBookings);
            ShowGamesCommand = new RelayCommand(ShowGames);
            ShowProfileCommand = new RelayCommand(ShowProfile);
            BookComputerCommand = new RelayCommand<Computer>(BookComputer);
            LaunchGameCommand = new RelayCommand<Client.Models.Game>(LaunchGame);
            ShowAdminPanelCommand = new RelayCommand(ShowAdminPanel);
            ConfirmAdminPasswordCommand = new RelayCommand<string>(ConfirmAdminPassword);

            // Загрузка тестовых данных
            LoadTestData();
            LoadConfig();

            // Подписываемся на изменения времени
            _timeService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TimeService.RemainingTime))
                    OnPropertyChanged(nameof(RemainingTime));
                else if (e.PropertyName == nameof(TimeService.Balance))
                    OnPropertyChanged(nameof(Balance));
            };
        }

        public string Username => _authManager.CurrentUser?.Username ?? "Гость";
        public decimal Balance => _timeService.Balance;
        public string RemainingTime => _timeService.FormattedRemainingTime;

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
                    Status = SharedStatus.Available
                },
                new Computer
                {
                    Id = 2,
                    Name = "PC Gamer Ultra",
                    Specifications = "RTX 4090, i9-14900K, 64GB RAM",
                    IsAvailable = true,
                    PricePerHour = 300,
                    Status = SharedStatus.Available
                },
                new Computer
                {
                    Id = 3,
                    Name = "PC Gamer Standard",
                    Specifications = "RTX 3060, i5-13600K, 16GB RAM",
                    IsAvailable = false,
                    PricePerHour = 150,
                    Status = SharedStatus.InUse
                }
            };
        }

        private void LoadConfig()
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                _gameConfig = JsonSerializer.Deserialize<GameConfig>(json);
                Games = new ObservableCollection<Client.Models.Game>(_gameConfig.Games);
            }
            else
            {
                // Обработка отсутствия файла конфигурации
                Games = new ObservableCollection<Client.Models.Game>();
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

        private void LaunchGame(Client.Models.Game game)
        {
            if (game != null && game.IsAvailable)
            {
                // Проверка наличия времени перед запуском игры
                if (_timeService.RemainingTime <= TimeSpan.Zero)
                {
                    System.Windows.MessageBox.Show("Невозможно запустить игру: у вас закончилось время. Пожалуйста, пополните время.", 
                        "Доступ запрещен", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = game.ExecutablePath,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при запуске игры: {ex.Message}", 
                        "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
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


