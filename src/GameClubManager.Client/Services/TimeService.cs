using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using GameClubManager.Client.Models;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace GameClubManager.Client.Services
{
    public class TimeService : INotifyPropertyChanged
    {
        private static TimeService? _instance;
        private readonly DispatcherTimer _timer;
        private decimal _balance;
        private TimeSpan _remainingTime;
        private readonly ApiService _apiService;
        private int? _currentUserId;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private DateTime _lastTick = DateTime.Now;

        public static TimeService Instance => _instance ??= new TimeService();

        private TimeService()
        {
            _apiService = ApiService.Instance;
            
            // Таймер для отсчета времени - используем более короткий интервал для гарантированного обновления
            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += Timer_Tick;
            
            // Устанавливаем тестовые данные с самого начала
            _balance = 750;
            _remainingTime = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30));
            
            // Запускаем таймер сразу
            StartTimer();
            
            Trace.WriteLine($"TimeService создан: {DateTime.Now}, баланс: {_balance}, время: {FormattedRemainingTime}");
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged(nameof(Balance));
                }
            }
        }

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged(nameof(RemainingTime));
                    OnPropertyChanged(nameof(FormattedRemainingTime));
                    
                    Trace.WriteLine($"RemainingTime изменено: {FormattedRemainingTime}");
                }
            }
        }

        public string FormattedRemainingTime
        {
            get
            {
                if (RemainingTime.TotalHours >= 1)
                {
                    return $"{RemainingTime.Hours}ч {RemainingTime.Minutes}м {RemainingTime.Seconds}с";
                }
                return $"{RemainingTime.Minutes}м {RemainingTime.Seconds}с";
            }
        }

        public async Task LoadUserDataAsync(int userId)
        {
            // Используем семафор для избежания состояния гонки
            await _semaphore.WaitAsync();
            
            try
            {
                Trace.WriteLine($"Загрузка данных пользователя {userId} началась: {DateTime.Now}");
                
                // Останавливаем предыдущий таймер
                StopTimer();
                
                var userData = await _apiService.GetUserDataAsync(userId);
                if (userData != null)
                {
                    _currentUserId = userId;
                    
                    // Устанавливаем значения из БД
                    Balance = userData.Balance;
                    RemainingTime = userData.RemainingTime;
                    
                    Trace.WriteLine($"Данные пользователя {userId} загружены: баланс {Balance}, время {FormattedRemainingTime}");
                }
                else
                {
                    // Если данные не получены, создаем новые тестовые данные
                    _currentUserId = userId;
                    Balance = 750;
                    RemainingTime = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30));
                    
                    // Сохраняем эти данные на сервере
                    await SaveUserData();
                    
                    Trace.WriteLine($"Созданы тестовые данные для пользователя {userId}: баланс {Balance}, время {FormattedRemainingTime}");
                }
                
                // Запускаем таймер снова, если есть оставшееся время
                if (RemainingTime > TimeSpan.Zero)
                {
                    StartTimer();
                }
            }
            catch (Exception ex)
            {
                // В случае ошибки используем тестовые данные
                _currentUserId = userId;
                Balance = 750;
                RemainingTime = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30));
                
                // Запускаем таймер
                StartTimer();
                
                Trace.WriteLine($"Ошибка при загрузке данных пользователя {userId}: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
            
            // Явно вызываем события изменения для обновления UI
            Application.Current.Dispatcher.Invoke(() => {
                OnPropertyChanged(nameof(Balance));
                OnPropertyChanged(nameof(RemainingTime));
                OnPropertyChanged(nameof(FormattedRemainingTime));
                
                Trace.WriteLine($"События PropertyChanged вызваны принудительно: {DateTime.Now}");
            });
        }

        // Оставляем старый метод для обратной совместимости
        public void LoadUserData(int userId)
        {
            // Просто вызываем асинхронную версию и игнорируем результат
            _ = LoadUserDataAsync(userId);
        }

        public async Task SaveUserData()
        {
            try
            {
                if (_currentUserId.HasValue)
                {
                    var userData = new UserData
                    {
                        Balance = Balance,
                        RemainingTime = RemainingTime
                    };
                    await _apiService.UpdateUserDataAsync(_currentUserId.Value, userData);
                    Trace.WriteLine($"Данные пользователя {_currentUserId.Value} сохранены: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка сохранения данных пользователя: {ex.Message}");
            }
        }

        public void AddBalance(decimal amount)
        {
            Balance += amount;
            _ = SaveUserData();
        }

        public void AddTime(TimeSpan time)
        {
            RemainingTime += time;
            
            // Если таймер не запущен, но появилось время - запускаем
            if (!_timer.IsEnabled && RemainingTime > TimeSpan.Zero)
            {
                StartTimer();
            }
            
            _ = SaveUserData();
        }

        private void StartTimer()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                Trace.WriteLine($"Таймер запущен: {DateTime.Now}");
            }
        }

        private void StopTimer()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                Trace.WriteLine($"Таймер остановлен: {DateTime.Now}");
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Проверяем, прошла ли секунда с прошлого тика
            var now = DateTime.Now;
            var elapsed = now - _lastTick;
            
            // Если прошло меньше секунды, выходим
            if (elapsed.TotalSeconds < 1.0)
            {
                return;
            }
            
            _lastTick = now;
            
            if (RemainingTime > TimeSpan.Zero)
            {
                // Отнимаем секунду
                var newRemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));
                
                // Устанавливаем новое значение
                RemainingTime = newRemainingTime;
                
                // Логгируем изменение
                Trace.WriteLine($"Таймер тикает: {FormattedRemainingTime}, время: {DateTime.Now}");
                
                // Каждую минуту сохраняем данные
                if (RemainingTime.Seconds == 0)
                {
                    _ = SaveUserData();
                }
                
                // Принудительно уведомляем об изменении на UI потоке
                Application.Current.Dispatcher.Invoke(() => {
                    OnPropertyChanged(nameof(FormattedRemainingTime));
                });
            }
            else
            {
                StopTimer();
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    MessageBox.Show("Время истекло!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            Trace.WriteLine($"OnPropertyChanged вызван для {propertyName}: {DateTime.Now}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 