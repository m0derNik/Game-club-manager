using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Linq;
using GameClubManager.Shared.Models;
using System.Collections.Generic;

namespace GameClubManager.Admin.ViewModels
{
    public class NotificationsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Models.AdminNotification> _notifications;
        private Models.AdminNotification _selectedNotification;
        private readonly ApiService _apiService;
        private readonly ComputerService _computerService;
        private DispatcherTimer _refreshTimer;
        private bool _isLoading;
        private bool _hasAdminCallNotifications;
        private bool _isDataVisible = true;

        public NotificationsViewModel()
        {
            _apiService = ApiService.Instance;
            _computerService = ComputerService.Instance;
            Notifications = new ObservableCollection<Models.AdminNotification>();
            
            // Инициализируем команды
            HandleNotificationCommand = new RelayCommand<Models.AdminNotification>(ExecuteHandleNotification);
            IgnoreNotificationCommand = new RelayCommand<Models.AdminNotification>(ExecuteIgnoreNotification);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            
            // Настраиваем таймер для автоматического обновления уведомлений
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // Обновляем каждые 30 секунд
            };
            _refreshTimer.Tick += async (s, e) => await LoadNotificationsAsync();
            _refreshTimer.Start();
            
            // Загружаем уведомления при создании
            Task.Run(async () => await LoadNotificationsAsync());
        }

        public ObservableCollection<Models.AdminNotification> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged();
                // Обновляем зависимые свойства
                OnPropertyChanged(nameof(IsDataVisible));
                OnPropertyChanged(nameof(IsEmptyVisible));
            }
        }

        public Models.AdminNotification SelectedNotification
        {
            get => _selectedNotification;
            set
            {
                _selectedNotification = value;
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
                // Обновляем зависимые свойства
                OnPropertyChanged(nameof(IsDataVisible));
                OnPropertyChanged(nameof(IsEmptyVisible));
            }
        }

        // Свойство для управления видимостью таблицы данных
        public bool IsDataVisible => !_isLoading && (Notifications != null && Notifications.Count > 0);

        // Свойство для управления видимостью сообщения об отсутствии данных
        public bool IsEmptyVisible => !_isLoading && (Notifications == null || Notifications.Count == 0);

        // Свойство, указывающее, есть ли уведомления типа AdminCall
        public bool HasAdminCallNotifications
        {
            get => _hasAdminCallNotifications;
            set
            {
                if (_hasAdminCallNotifications != value)
                {
                    _hasAdminCallNotifications = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand HandleNotificationCommand { get; }
        public ICommand IgnoreNotificationCommand { get; }
        public ICommand RefreshCommand { get; }

        private async void ExecuteHandleNotification(Models.AdminNotification notification)
        {
            if (notification == null) return;
            
            // Если это вызов администратора, можно добавить дополнительную логику (например, открыть удаленный рабочий стол)
            if (notification.Type == "AdminCall" && notification.ComputerId.HasValue)
            {
                var computerService = ComputerService.Instance;
                var computerInfo = await computerService.GetComputerByIdAsync(notification.ComputerId.Value);
                
                if (computerInfo != null)
                {
                    var remoteService = RemoteDesktopService.Instance;
                    await remoteService.ConnectToComputerAsync(computerInfo.Name);
                }
            }
            
            // Отмечаем уведомление как прочитанное и удаляем его
            await MarkNotificationAsReadAsync(notification);
        }

        private async void ExecuteIgnoreNotification(Models.AdminNotification notification)
        {
            if (notification == null) return;
            
            // Отмечаем уведомление как прочитанное и удаляем его
            await MarkNotificationAsReadAsync(notification);
        }
        
        private async void ExecuteRefresh()
        {
            await LoadNotificationsAsync();
        }
        
        private async Task MarkNotificationAsReadAsync(Models.AdminNotification notification)
        {
            if (notification == null) return;
            
            // Удаляем уведомление из базы данных и из коллекции
            bool success = await _apiService.DeleteNotificationAsync(notification.Id);
            
            // Если успешно, удаляем уведомление из коллекции
            if (success)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Notifications.Remove(notification);
                    OnPropertyChanged(nameof(Notifications));
                    OnPropertyChanged(nameof(IsDataVisible));
                    OnPropertyChanged(nameof(IsEmptyVisible));
                });
            }
        }
        
        private async Task LoadNotificationsAsync()
        {
            try
            {
                IsLoading = true;
                
                // Получаем уведомления с сервера
                var serverNotifications = await _apiService.GetNotificationsAsync();
                
                // Загружаем информацию о компьютерах для вызовов администратора
                var adminCallNotifications = serverNotifications
                    .Where(n => n.Type == "AdminCall" && n.ComputerId.HasValue)
                    .ToList();
                    
                var computers = new Dictionary<int, Services.ComputerDto>();
                foreach (var notification in adminCallNotifications)
                {
                    if (notification.ComputerId.HasValue && !computers.ContainsKey(notification.ComputerId.Value))
                    {
                        try
                        {
                            var computer = await _computerService.GetComputerByIdAsync(notification.ComputerId.Value);
                            if (computer != null)
                            {
                                computers[notification.ComputerId.Value] = computer;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Ошибка при получении информации о компьютере {notification.ComputerId.Value}: {ex.Message}");
                        }
                    }
                }
                
                // Обновляем коллекцию в потоке UI
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Notifications.Clear();
                    
                    // Преобразуем уведомления и добавляем информацию о компьютерах
                    foreach (var notification in serverNotifications)
                    {
                        var adminNotification = notification.ToAdminModel();
                        
                        // Добавляем информацию о компьютере, если это вызов администратора
                        if (adminNotification.Type == "AdminCall" && adminNotification.ComputerId.HasValue)
                        {
                            if (computers.TryGetValue(adminNotification.ComputerId.Value, out var computer))
                            {
                                adminNotification.ComputerName = computer.Name ?? $"Компьютер #{adminNotification.ComputerId}";
                            }
                            else
                            {
                                adminNotification.ComputerName = $"Компьютер #{adminNotification.ComputerId}";
                            }
                        }
                        
                        Notifications.Add(adminNotification);
                    }
                    
                    // Обновляем флаг наличия вызовов администратора
                    HasAdminCallNotifications = Notifications.Any(n => n.Type == "AdminCall");
                    
                    // Проверяем на новые непрочитанные уведомления и показываем уведомление на рабочем столе
                    var unreadNotifications = serverNotifications.Where(n => !n.IsRead).ToList();
                    if (unreadNotifications.Any())
                    {
                        // Воспроизводим звук уведомления
                        System.Media.SystemSounds.Asterisk.Play();
                        
                        // Показываем всплывающее уведомление
                        if (unreadNotifications.Count == 1)
                        {
                            var notification = unreadNotifications.First();
                            System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon
                            {
                                Visible = true,
                                Icon = System.Drawing.SystemIcons.Information,
                                BalloonTipTitle = notification.Title,
                                BalloonTipText = notification.Message
                            };
                            notifyIcon.ShowBalloonTip(5000);
                            
                            // Удаляем иконку после показа
                            Task.Delay(6000).ContinueWith(_ => notifyIcon.Dispose());
                        }
                        else
                        {
                            System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon
                            {
                                Visible = true,
                                Icon = System.Drawing.SystemIcons.Information,
                                BalloonTipTitle = "Новые уведомления",
                                BalloonTipText = $"У вас {unreadNotifications.Count} новых уведомлений"
                            };
                            notifyIcon.ShowBalloonTip(5000);
                            
                            // Удаляем иконку после показа
                            Task.Delay(6000).ContinueWith(_ => notifyIcon.Dispose());
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке уведомлений: {ex.Message}", 
                    "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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