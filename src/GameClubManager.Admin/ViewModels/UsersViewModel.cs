using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;

namespace GameClubManager.Admin.ViewModels
{
    public class UsersViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ObservableCollection<User> _users;
        private ObservableCollection<User> _allUsers;
        private string _searchText;
        private User _selectedUser;
        private decimal _balanceToAdd;
        private int _hoursToAdd;
        private int _minutesToAdd;
        private bool _isLoading;
        private bool _isEmptyVisible;
        private bool _isDataVisible = true;

        public UsersViewModel()
        {
            _apiService = ApiService.Instance;
            
            // Инициализируем коллекции
            Users = new ObservableCollection<User>();
            _allUsers = new ObservableCollection<User>();
            
            // Инициализируем команды
            AddUserCommand = new RelayCommand(ExecuteAddUser);
            EditUserCommand = new RelayCommand<User>(ExecuteEditUser);
            DeleteUserCommand = new RelayCommand<User>(ExecuteDeleteUser);
            AddBalanceCommand = new RelayCommand<User>(ExecuteAddBalance);
            AddTimeCommand = new RelayCommand<User>(ExecuteAddTime);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
            // Инициализируем значения по умолчанию
            BalanceToAdd = 100;
            HoursToAdd = 1;
            MinutesToAdd = 0;
            
            // Загружаем пользователей
            _ = LoadUsersAsync();
        }

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
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
                FilterUsers();
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }
        
        public decimal BalanceToAdd
        {
            get => _balanceToAdd;
            set
            {
                _balanceToAdd = value;
                OnPropertyChanged();
            }
        }
        
        public int HoursToAdd
        {
            get => _hoursToAdd;
            set
            {
                _hoursToAdd = value;
                OnPropertyChanged();
            }
        }
        
        public int MinutesToAdd
        {
            get => _minutesToAdd;
            set
            {
                _minutesToAdd = value;
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

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand AddBalanceCommand { get; }
        public ICommand AddTimeCommand { get; }
        public ICommand RefreshCommand { get; }
        
        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                
                // Сохраняем текущий текст поиска
                var currentSearchText = SearchText;
                
                var users = await _apiService.GetAllUsersAsync();
                
                _allUsers.Clear();
                Users.Clear();
                
                foreach (var user in users)
                {
                    _allUsers.Add(user);
                }
                
                // Если у нас был текст поиска, применяем его снова
                if (!string.IsNullOrWhiteSpace(currentSearchText))
                {
                    SearchText = currentSearchText; // Это вызовет FilterUsers через привязку свойства
                }
                else
                {
                    // Иначе просто добавляем всех пользователей
                    foreach (var user in _allUsers)
                    {
                        Users.Add(user);
                    }
                    UpdateVisibility();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateVisibility()
        {
            IsEmptyVisible = !IsLoading && (Users == null || Users.Count == 0);
            IsDataVisible = !IsLoading && !IsEmptyVisible;
        }

        private void ExecuteRefresh()
        {
            _ = LoadUsersAsync();
        }

        private void ExecuteAddUser()
        {
            // В будущем здесь будет логика добавления пользователя через диалоговое окно
            MessageBox.Show("Функция добавления пользователя будет доступна в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteEditUser(User user)
        {
            // В будущем здесь будет логика редактирования пользователя через диалоговое окно
            MessageBox.Show($"Редактирование пользователя: {user.Name}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ExecuteDeleteUser(User user)
        {
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить пользователя {user.Name}?", 
                "Подтверждение удаления", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    
                    bool success = await _apiService.DeleteUserAsync(user.Id);
                    
                    if (success)
                    {
                        _allUsers.Remove(user);
                        Users.Remove(user);
                        MessageBox.Show($"Пользователь {user.Name} успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        
        private void ExecuteAddBalance(User user)
        {
            // Создаем простой диалог для ввода суммы
            var dialog = new Window
            {
                Title = $"Пополнить баланс - {user.Name}",
                Width = 400,
                Height = 200,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = (Brush)Application.Current.Resources["BackgroundBrush"]
            };
            
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var label = new TextBlock
            {
                Text = "Введите сумму для пополнения:",
                Margin = new Thickness(0, 0, 0, 10),
                Style = (Style)Application.Current.Resources["BodyTextStyle"],
                Foreground = Brushes.White
            };
            
            var textBox = new TextBox
            {
                Text = BalanceToAdd.ToString(),
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Foreground = Brushes.White,
                CaretBrush = Brushes.White
            };
            
            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            
            var cancelButton = new Button
            {
                Content = "Отмена",
                Margin = new Thickness(0, 0, 10, 0),
                Style = (Style)Application.Current.Resources["MaterialDesignOutlinedButton"],
                Foreground = Brushes.White
            };
            
            var confirmButton = new Button
            {
                Content = "Пополнить",
                Style = (Style)Application.Current.Resources["MaterialDesignRaisedButton"],
                Foreground = Brushes.White
            };
            
            cancelButton.Click += (s, e) => dialog.DialogResult = false;
            confirmButton.Click += async (s, e) =>
            {
                if (decimal.TryParse(textBox.Text, out decimal amount) && amount > 0)
                {
                    try
                    {
                        confirmButton.IsEnabled = false;
                        cancelButton.IsEnabled = false;
                        
                        bool success = await _apiService.AddUserBalanceAsync(user.Id, amount);
                        
                        if (success)
                        {
                            user.Balance += amount;
                            OnPropertyChanged(nameof(Users));
                            dialog.DialogResult = true;
                        }
                        else
                        {
                            dialog.DialogResult = false;
                        }
                    }
                    finally
                    {
                        confirmButton.IsEnabled = true;
                        cancelButton.IsEnabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(confirmButton);
            
            Grid.SetRow(label, 0);
            Grid.SetRow(textBox, 1);
            Grid.SetRow(buttonsPanel, 2);
            
            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonsPanel);
            
            dialog.Content = grid;
            
            var result = dialog.ShowDialog();
            
            if (result == true)
            {
                MessageBox.Show($"Баланс пользователя {user.Name} успешно пополнен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void ExecuteAddTime(User user)
        {
            // Создаем диалог для ввода времени
            var dialog = new Window
            {
                Title = $"Добавить время - {user.Name}",
                Width = 400,
                Height = 250,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = (Brush)Application.Current.Resources["BackgroundBrush"]
            };
            
            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var label = new TextBlock
            {
                Text = "Введите время для добавления:",
                Margin = new Thickness(0, 0, 0, 10),
                Style = (Style)Application.Current.Resources["BodyTextStyle"],
                Foreground = Brushes.White
            };
            
            var timePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
            
            var hoursPanel = new StackPanel { Margin = new Thickness(0, 0, 20, 0) };
            var hoursLabel = new TextBlock { 
                Text = "Часы:", 
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = Brushes.White
            };
            var hoursBox = new TextBox { 
                Text = HoursToAdd.ToString(), 
                Width = 100,
                Foreground = Brushes.White,
                CaretBrush = Brushes.White
            };
            hoursPanel.Children.Add(hoursLabel);
            hoursPanel.Children.Add(hoursBox);
            
            var minutesPanel = new StackPanel();
            var minutesLabel = new TextBlock { 
                Text = "Минуты:", 
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = Brushes.White
            };
            var minutesBox = new TextBox { 
                Text = MinutesToAdd.ToString(), 
                Width = 100,
                Foreground = Brushes.White,
                CaretBrush = Brushes.White
            };
            minutesPanel.Children.Add(minutesLabel);
            minutesPanel.Children.Add(minutesBox);
            
            timePanel.Children.Add(hoursPanel);
            timePanel.Children.Add(minutesPanel);
            
            var currentTimeLabel = new TextBlock
            {
                Text = $"Текущее время: {user.FormattedRemainingTime}",
                Margin = new Thickness(0, 0, 0, 20),
                Style = (Style)Application.Current.Resources["BodyTextStyle"],
                Foreground = Brushes.White
            };
            
            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            
            var cancelButton = new Button
            {
                Content = "Отмена",
                Margin = new Thickness(0, 0, 10, 0),
                Style = (Style)Application.Current.Resources["MaterialDesignOutlinedButton"],
                Foreground = Brushes.White
            };
            
            var confirmButton = new Button
            {
                Content = "Добавить время",
                Style = (Style)Application.Current.Resources["MaterialDesignRaisedButton"],
                Foreground = Brushes.White
            };
            
            cancelButton.Click += (s, e) => dialog.DialogResult = false;
            confirmButton.Click += async (s, e) =>
            {
                if (int.TryParse(hoursBox.Text, out int hours) && int.TryParse(minutesBox.Text, out int minutes))
                {
                    if (hours >= 0 && minutes >= 0 && minutes < 60 && (hours > 0 || minutes > 0))
                    {
                        try
                        {
                            confirmButton.IsEnabled = false;
                            cancelButton.IsEnabled = false;
                            
                            var timeToAdd = TimeSpan.FromHours(hours).Add(TimeSpan.FromMinutes(minutes));
                            bool success = await _apiService.AddUserTimeAsync(user.Id, timeToAdd);
                            
                            if (success)
                            {
                                user.RemainingTime = user.RemainingTime.Add(timeToAdd);
                                OnPropertyChanged(nameof(Users));
                                dialog.DialogResult = true;
                            }
                            else
                            {
                                dialog.DialogResult = false;
                            }
                        }
                        finally
                        {
                            confirmButton.IsEnabled = true;
                            cancelButton.IsEnabled = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Введите корректное время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(confirmButton);
            
            Grid.SetRow(label, 0);
            Grid.SetRow(timePanel, 1);
            Grid.SetRow(currentTimeLabel, 2);
            Grid.SetRow(buttonsPanel, 3);
            
            grid.Children.Add(label);
            grid.Children.Add(timePanel);
            grid.Children.Add(currentTimeLabel);
            grid.Children.Add(buttonsPanel);
            
            dialog.Content = grid;
            
            var result = dialog.ShowDialog();
            
            if (result == true)
            {
                MessageBox.Show($"Время пользователя {user.Name} успешно добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void ShowSearchResults()
        {
            // Если был применен поиск и нет результатов, показываем сообщение
            if (!string.IsNullOrWhiteSpace(SearchText) && Users.Count == 0)
            {
                MessageBox.Show($"По запросу \"{SearchText}\" не найдено ни одного пользователя.", 
                              "Результаты поиска", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            // Если найдено более 10 результатов, тоже показываем сообщение
            else if (!string.IsNullOrWhiteSpace(SearchText) && Users.Count > 10)
            {
                var pluralEnding = "ей";
                MessageBox.Show($"Найдено {Users.Count} пользовател{pluralEnding} по запросу: \"{SearchText}\"\nПопробуйте уточнить запрос для получения более точных результатов.", 
                              "Большое количество результатов", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Если поисковая строка пуста, показываем всех пользователей
                Users.Clear();
                foreach (var user in _allUsers)
                {
                    Users.Add(user);
                }
                
                UpdateVisibility();
                return;
            }
            
            // Фильтруем пользователей по разным полям
            var searchTerms = SearchText.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Users.Clear();
            
            foreach (var user in _allUsers)
            {
                // Проверяем каждый поисковый термин отдельно
                bool matchesAllTerms = true;
                
                foreach (var term in searchTerms)
                {
                    // Проверяем, соответствует ли пользователь текущему поисковому термину
                    bool matchesTerm = 
                        (user.Name?.ToLower().Contains(term) == true) || 
                        (user.Username?.ToLower().Contains(term) == true) || 
                        (user.Email?.ToLower().Contains(term) == true) ||
                        (user.Role?.ToLower().Contains(term) == true) ||
                        (user.Status?.ToLower().Contains(term) == true) ||
                        (user.Balance.ToString().Contains(term));
                    
                    // Если не соответствует хотя бы одному термину, пользователь не подходит
                    if (!matchesTerm)
                    {
                        matchesAllTerms = false;
                        break;
                    }
                }
                
                // Если пользователь соответствует всем поисковым терминам, добавляем его в результаты
                if (matchesAllTerms)
                {
                    Users.Add(user);
                }
            }
            
            UpdateVisibility();
            
            // Вызываем метод для отображения результатов поиска
            ShowSearchResults();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 