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
                System.Windows.MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            System.Windows.MessageBox.Show("Функция добавления пользователя будет доступна в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteEditUser(User user)
        {
            // В будущем здесь будет логика редактирования пользователя через диалоговое окно
            System.Windows.MessageBox.Show($"Редактирование пользователя: {user.Name}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ExecuteDeleteUser(User user)
        {
            var result = System.Windows.MessageBox.Show(
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
                        System.Windows.MessageBox.Show($"Пользователь {user.Name} успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        
        private void ExecuteAddBalance(User user)
        {
            if (user == null || BalanceToAdd <= 0)
                return;
            
            // Создаем диалоговое окно для ввода суммы
            var dialog = new Window
            {
                Title = $"Пополнить баланс - {user.Name}",
                Width = 400,
                Height = 250,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize,
                Icon = System.Windows.Application.Current.MainWindow.Icon
            };
            
            // Создаем Grid с 2 колонками
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.Margin = new Thickness(20);
            
            // Добавляем элементы для текущего баланса
            var currentBalanceLabel = new TextBlock
            {
                Text = "Текущий баланс:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var currentBalanceValue = new TextBlock
            {
                Text = $"{user.Balance:C}",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Green
            };
            
            // Добавляем элементы для ввода суммы пополнения
            var amountLabel = new TextBlock
            {
                Text = "Сумма пополнения:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var amountTextBox = new System.Windows.Controls.TextBox
            {
                Text = BalanceToAdd.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(5)
            };
            
            // Добавляем элементы для отображения нового баланса
            var newBalanceLabel = new TextBlock
            {
                Text = "Новый баланс:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var newBalanceValue = new TextBlock
            {
                Text = $"{user.Balance + BalanceToAdd:C}",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Blue
            };
            
            // Обновляем новый баланс при изменении суммы
            amountTextBox.TextChanged += (s, e) =>
            {
                if (decimal.TryParse(amountTextBox.Text, out decimal amount))
                {
                    newBalanceValue.Text = $"{user.Balance + amount:C}";
                    newBalanceValue.Foreground = amount > 0 ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Red;
                }
            };
            
            // Добавляем кнопки
            var buttonsPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            
            var confirmButton = new System.Windows.Controls.Button
            {
                Content = "Пополнить",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White
            };
            
            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Отмена",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(10)
            };
            
            // Добавляем обработчики для кнопок
            confirmButton.Click += async (s, e) =>
            {
                if (decimal.TryParse(amountTextBox.Text, out decimal amount) && amount > 0)
                {
                    try
                    {
                        // Вызываем API для пополнения баланса
                        bool success = await _apiService.AddUserBalanceAsync(user.Id, amount);
                        
                        if (success)
                        {
                            // Обновляем баланс пользователя
                            user.Balance += amount;
                            
                            System.Windows.MessageBox.Show($"Баланс пользователя {user.Name} успешно пополнен на {amount:C}", 
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                            dialog.DialogResult = true;
                            dialog.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка при пополнении баланса: {ex.Message}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Пожалуйста, введите корректную сумму пополнения", 
                        "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            
            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };
            
            // Добавляем элементы в Grid
            Grid.SetRow(currentBalanceLabel, 0);
            Grid.SetColumn(currentBalanceLabel, 0);
            grid.Children.Add(currentBalanceLabel);
            
            Grid.SetRow(currentBalanceValue, 0);
            Grid.SetColumn(currentBalanceValue, 1);
            grid.Children.Add(currentBalanceValue);
            
            Grid.SetRow(amountLabel, 1);
            Grid.SetColumn(amountLabel, 0);
            grid.Children.Add(amountLabel);
            
            Grid.SetRow(amountTextBox, 1);
            Grid.SetColumn(amountTextBox, 1);
            grid.Children.Add(amountTextBox);
            
            Grid.SetRow(newBalanceLabel, 2);
            Grid.SetColumn(newBalanceLabel, 0);
            grid.Children.Add(newBalanceLabel);
            
            Grid.SetRow(newBalanceValue, 2);
            Grid.SetColumn(newBalanceValue, 1);
            grid.Children.Add(newBalanceValue);
            
            buttonsPanel.Children.Add(confirmButton);
            buttonsPanel.Children.Add(cancelButton);
            
            Grid.SetRow(buttonsPanel, 3);
            Grid.SetColumnSpan(buttonsPanel, 2);
            grid.Children.Add(buttonsPanel);
            
            // Устанавливаем содержимое диалога
            dialog.Content = grid;
            
            // Показываем диалог
            dialog.ShowDialog();
        }
        
        private void ExecuteAddTime(User user)
        {
            if (user == null || (HoursToAdd == 0 && MinutesToAdd == 0))
                return;
            
            // Создаем диалоговое окно для ввода времени
            var dialog = new Window
            {
                Title = $"Добавить время - {user.Name}",
                Width = 400,
                Height = 280,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize,
                Icon = System.Windows.Application.Current.MainWindow.Icon
            };
            
            // Создаем Grid с 2 колонками
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            grid.Margin = new Thickness(20);
            
            // Добавляем элементы для текущего времени
            var currentTimeLabel = new TextBlock
            {
                Text = "Текущее время:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var currentTimeValue = new TextBlock
            {
                Text = $"{user.RemainingTime.Hours:D2}:{user.RemainingTime.Minutes:D2}:{user.RemainingTime.Seconds:D2}",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Green
            };
            
            // Добавляем элементы для ввода часов
            var hoursLabel = new TextBlock
            {
                Text = "Часы:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var hoursTextBox = new System.Windows.Controls.TextBox
            {
                Text = HoursToAdd.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(5)
            };
            
            // Добавляем элементы для ввода минут
            var minutesLabel = new TextBlock
            {
                Text = "Минуты:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var minutesTextBox = new System.Windows.Controls.TextBox
            {
                Text = MinutesToAdd.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(5)
            };
            
            // Добавляем элементы для отображения нового времени
            var newTimeLabel = new TextBlock
            {
                Text = "Новое время:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            var newTime = user.RemainingTime.Add(new TimeSpan(HoursToAdd, MinutesToAdd, 0));
            var newTimeValue = new TextBlock
            {
                Text = $"{newTime.Hours:D2}:{newTime.Minutes:D2}:{newTime.Seconds:D2}",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Blue
            };
            
            // Обновляем новое время при изменении значений
            void UpdateNewTime()
            {
                if (int.TryParse(hoursTextBox.Text, out int hours) && int.TryParse(minutesTextBox.Text, out int minutes))
                {
                    var additionalTime = new TimeSpan(hours, minutes, 0);
                    var newTime = user.RemainingTime.Add(additionalTime);
                    newTimeValue.Text = $"{newTime.Hours:D2}:{newTime.Minutes:D2}:{newTime.Seconds:D2}";
                    newTimeValue.Foreground = additionalTime.TotalMinutes > 0 ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Red;
                }
            }
            
            hoursTextBox.TextChanged += (s, e) => UpdateNewTime();
            minutesTextBox.TextChanged += (s, e) => UpdateNewTime();
            
            // Добавляем кнопки
            var buttonsPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            
            var confirmButton = new System.Windows.Controls.Button
            {
                Content = "Добавить время",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White
            };
            
            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Отмена",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(10)
            };
            
            // Добавляем обработчики для кнопок
            confirmButton.Click += async (s, e) =>
            {
                if (int.TryParse(hoursTextBox.Text, out int hours) && int.TryParse(minutesTextBox.Text, out int minutes))
                {
                    var timeToAdd = new TimeSpan(hours, minutes, 0);
                    if (timeToAdd.TotalMinutes > 0)
                    {
                        try
                        {
                            // Вызываем API для добавления времени
                            bool success = await _apiService.AddUserTimeAsync(user.Id, timeToAdd);
                            
                            if (success)
                            {
                                // Обновляем время пользователя
                                user.RemainingTime = user.RemainingTime.Add(timeToAdd);
                                
                                System.Windows.MessageBox.Show($"Время пользователя {user.Name} успешно увеличено на {timeToAdd.Hours}ч {timeToAdd.Minutes}м", 
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                    
                                dialog.DialogResult = true;
                                dialog.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Ошибка при добавлении времени: {ex.Message}", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Время должно быть положительным", 
                            "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Пожалуйста, введите корректные значения времени", 
                        "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            
            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };
            
            // Добавляем элементы в Grid
            Grid.SetRow(currentTimeLabel, 0);
            Grid.SetColumn(currentTimeLabel, 0);
            grid.Children.Add(currentTimeLabel);
            
            Grid.SetRow(currentTimeValue, 0);
            Grid.SetColumn(currentTimeValue, 1);
            grid.Children.Add(currentTimeValue);
            
            Grid.SetRow(hoursLabel, 1);
            Grid.SetColumn(hoursLabel, 0);
            grid.Children.Add(hoursLabel);
            
            Grid.SetRow(hoursTextBox, 1);
            Grid.SetColumn(hoursTextBox, 1);
            grid.Children.Add(hoursTextBox);
            
            Grid.SetRow(minutesLabel, 2);
            Grid.SetColumn(minutesLabel, 0);
            grid.Children.Add(minutesLabel);
            
            Grid.SetRow(minutesTextBox, 2);
            Grid.SetColumn(minutesTextBox, 1);
            grid.Children.Add(minutesTextBox);
            
            Grid.SetRow(newTimeLabel, 3);
            Grid.SetColumn(newTimeLabel, 0);
            grid.Children.Add(newTimeLabel);
            
            Grid.SetRow(newTimeValue, 3);
            Grid.SetColumn(newTimeValue, 1);
            grid.Children.Add(newTimeValue);
            
            buttonsPanel.Children.Add(confirmButton);
            buttonsPanel.Children.Add(cancelButton);
            
            Grid.SetRow(buttonsPanel, 4);
            Grid.SetColumnSpan(buttonsPanel, 2);
            grid.Children.Add(buttonsPanel);
            
            // Устанавливаем содержимое диалога
            dialog.Content = grid;
            
            // Показываем диалог
            dialog.ShowDialog();
        }
        
        private void ShowSearchResults()
        {
            // Если был применен поиск и нет результатов, показываем сообщение
            if (!string.IsNullOrWhiteSpace(SearchText) && Users.Count == 0)
            {
                System.Windows.MessageBox.Show($"По запросу \"{SearchText}\" не найдено ни одного пользователя.", 
                              "Результаты поиска", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            // Если найдено более 10 результатов, тоже показываем сообщение
            else if (!string.IsNullOrWhiteSpace(SearchText) && Users.Count > 10)
            {
                var pluralEnding = "ей";
                System.Windows.MessageBox.Show($"Найдено {Users.Count} пользовател{pluralEnding} по запросу: \"{SearchText}\"\nПопробуйте уточнить запрос для получения более точных результатов.", 
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