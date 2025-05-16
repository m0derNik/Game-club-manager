using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;
using GameClubManager.Shared.Models;
using AdminGame = GameClubManager.Admin.Models.Game;
using SharedGame = GameClubManager.Shared.Models.Game;
using MessageBox = System.Windows.MessageBox;
using System.Linq;

namespace GameClubManager.Admin.ViewModels
{
    public class GamesViewModel : INotifyPropertyChanged
    {
        private readonly GameService _gameService;
        private AdminGame _selectedGame;
        private bool _isLoading;
        private bool _isEditing;
        private AdminGame _editingGame;
        private string _errorMessage;

        public GamesViewModel()
        {
            _gameService = GameService.Instance;
            
            // Команды
            LoadCommand = new AsyncRelayCommand(LoadGamesAsync);
            AddCommand = new RelayCommand(AddGame, () => !IsEditing);
            EditCommand = new RelayCommand(EditGame, () => SelectedGame != null && !IsEditing);
            SaveCommand = new AsyncRelayCommand(SaveGameAsync, () => IsEditing && IsFormValid());
            CancelCommand = new RelayCommand(CancelEdit, () => IsEditing);
            DeleteCommand = new AsyncRelayCommand<int>(DeleteGameAsync);
            DeleteEditingGameCommand = new AsyncRelayCommand(DeleteEditingGameAsync, () => IsEditing && EditingGame != null && EditingGame.Id > 0);
            ToggleAvailabilityCommand = new AsyncRelayCommand<int>(ToggleGameAvailabilityAsync);
            
            // Загружаем игры при создании
            _ = LoadGamesAsync();
        }

        // Свойства
        public ObservableCollection<AdminGame> Games => _gameService.Games;

        public AdminGame SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
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

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAdding));
                CommandManager.InvalidateRequerySuggested();
                
                // Обновляем состояние кнопки удаления
                (DeleteEditingGameCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public AdminGame EditingGame
        {
            get => _editingGame;
            set
            {
                _editingGame = value;
                OnPropertyChanged();
                
                // Обновляем состояние кнопок
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (DeleteEditingGameCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
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

        public bool IsAdding => IsEditing && EditingGame?.Id == 0;

        // Команды
        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DeleteEditingGameCommand { get; }
        public ICommand ToggleAvailabilityCommand { get; }

        // Массив всех значений перечисления GameGenre для использования в ComboBox
        public Array GameGenres => Enum.GetValues(typeof(GameGenre));

        // Методы
        private async Task LoadGamesAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                await _gameService.LoadGamesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки игр: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddGame()
        {
            IsEditing = true;
            EditingGame = new AdminGame
            {
                Name = "Новая игра",
                Description = string.Empty,
                Genre = GameGenre.Action,
                ExecutablePath = string.Empty,
                IsAvailable = true
            };
            
            // Подписываемся на изменения свойств
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }

        private void EditGame()
        {
            if (SelectedGame == null) return;
            
            IsEditing = true;
            EditingGame = new AdminGame
            {
                Id = SelectedGame.Id,
                Name = SelectedGame.Name,
                Description = SelectedGame.Description,
                Genre = SelectedGame.Genre,
                ExecutablePath = SelectedGame.ExecutablePath,
                IsAvailable = SelectedGame.IsAvailable
            };
            
            // Подписываемся на изменения свойств
            EditingGame.PropertyChanged += EditingGame_PropertyChanged;
        }
        
        private void EditingGame_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Обновляем состояние кнопки сохранения при изменении свойств редактируемой игры
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool IsFormValid()
        {
            if (EditingGame == null) return false;
            
            if (string.IsNullOrWhiteSpace(EditingGame.Name))
                return false;
                
            if (string.IsNullOrWhiteSpace(EditingGame.ExecutablePath))
                return false;
                
            return true;
        }

        private async Task SaveGameAsync()
        {
            if (EditingGame == null) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                // Проверяем обязательные поля
                if (string.IsNullOrWhiteSpace(EditingGame.Name))
                {
                    ErrorMessage = "Название игры не может быть пустым";
                    return;
                }

                if (string.IsNullOrWhiteSpace(EditingGame.ExecutablePath))
                {
                    ErrorMessage = "Путь к исполняемому файлу не может быть пустым";
                    return;
                }

                bool success;
                
                if (EditingGame.Id == 0)
                {
                    // Добавление новой игры
                    await _gameService.AddGameAsync(EditingGame);
                    success = true;
                }
                else
                {
                    // Обновление существующей игры
                    success = await _gameService.UpdateGameAsync(EditingGame);
                }
                
                if (success)
                {
                    IsEditing = false;
                    EditingGame = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения игры: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            if (EditingGame != null)
            {
                // Отписываемся от событий
                EditingGame.PropertyChanged -= EditingGame_PropertyChanged;
            }
            
            IsEditing = false;
            EditingGame = null;
            ErrorMessage = string.Empty;
        }

        private async Task DeleteGameAsync(int gameId)
        {
            // Находим игру для удаления
            var gameToDelete = _gameService.Games.FirstOrDefault(g => g.Id == gameId);
            if (gameToDelete == null) return;
            
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить игру '{gameToDelete.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                
            if (result != MessageBoxResult.Yes) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                var success = await _gameService.DeleteGameAsync(gameId);
                
                if (!success)
                {
                    ErrorMessage = "Не удалось удалить игру";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления игры: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteEditingGameAsync()
        {
            if (EditingGame == null || EditingGame.Id == 0) return;
            
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить игру '{EditingGame.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                
            if (result != MessageBoxResult.Yes) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                var success = await _gameService.DeleteGameAsync(EditingGame.Id);
                
                if (success)
                {
                    // При успешном удалении закрываем форму редактирования
                    CancelEdit();
                }
                else
                {
                    ErrorMessage = "Не удалось удалить игру";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления игры: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleGameAvailabilityAsync(int gameId)
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                var success = await _gameService.ToggleGameAvailabilityAsync(gameId);
                
                if (!success)
                {
                    ErrorMessage = "Не удалось изменить доступность игры";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка изменения доступности игры: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 