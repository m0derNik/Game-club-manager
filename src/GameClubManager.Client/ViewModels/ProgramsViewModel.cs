using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Client.Models;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Services;
using System.Diagnostics;
using System.Linq;

namespace GameClubManager.Client.ViewModels
{
    public class ProgramsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly TimeService _timeService;
        private ObservableCollection<Game> _games;
        private ObservableCollection<Game> _allGames;
        private bool _isLoading;
        private string _errorMessage;
        private string _searchText;

        public ObservableCollection<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterGames();
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LaunchGameCommand { get; }
        public ICommand RefreshGamesCommand { get; }

        public ProgramsViewModel()
        {
            _apiService = ApiService.Instance;
            _timeService = TimeService.Instance;
            _allGames = new ObservableCollection<Game>();
            Games = new ObservableCollection<Game>();
            LaunchGameCommand = new RelayCommand<Game>(LaunchGame);
            RefreshGamesCommand = new AsyncRelayCommand(LoadGamesAsync);
            
            // Загружаем игры при создании
            _ = LoadGamesAsync();
        }

        private void FilterGames()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Если строка поиска пуста, показываем все игры
                Games = new ObservableCollection<Game>(_allGames);
            }
            else
            {
                // Фильтруем игры по имени и описанию
                var filteredGames = _allGames.Where(g => 
                    g.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                    g.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                Games = new ObservableCollection<Game>(filteredGames);
            }
        }

        private async Task LoadGamesAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var games = await _apiService.GetGamesForUserAsync();
                _allGames.Clear();
                
                if (games != null)
                {
                    foreach (var game in games)
                    {
                        _allGames.Add(game);
                    }
                }
                
                // Применяем фильтр к загруженным играм
                FilterGames();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке игр: {ex.Message}";
                ShowErrorMessage(ErrorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowErrorMessage(string errorMessage)
        {
            ErrorMessage = errorMessage;
            IsLoading = false;
            System.Windows.MessageBox.Show(
                errorMessage,
                "Ошибка",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }

        private void LaunchGame(Game game)
        {
            if (game == null || string.IsNullOrEmpty(game.ExecutablePath))
            {
                System.Windows.MessageBox.Show("Путь к игре не указан", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            // Проверка наличия времени перед запуском игры
            if (_timeService.RemainingTime <= TimeSpan.Zero)
            {
                System.Windows.MessageBox.Show("Невозможно запустить игру: у вас закончилось время. Пожалуйста, пополните время.", 
                    "Доступ запрещен", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = game.ExecutablePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при запуске игры: {ex.Message}";
                System.Windows.MessageBox.Show($"Не удалось запустить игру: {ex.Message}", 
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 


