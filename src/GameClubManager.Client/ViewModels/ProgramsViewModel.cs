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

namespace GameClubManager.Client.ViewModels
{
    public class ProgramsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Game> _games;
        private bool _isLoading;
        private string _errorMessage;

        public ObservableCollection<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
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
            Games = new ObservableCollection<Game>();
            LaunchGameCommand = new RelayCommand<Game>(LaunchGame);
            RefreshGamesCommand = new AsyncRelayCommand(LoadGamesAsync);
            
            // Загружаем игры при создании
            _ = LoadGamesAsync();
        }

        private async Task LoadGamesAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var games = await _apiService.GetGamesForUserAsync();
                Games.Clear();
                
                if (games != null)
                {
                    foreach (var game in games)
                    {
                        Games.Add(game);
                    }
                }
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


