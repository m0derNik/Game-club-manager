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
            
            // Создаем таймер, который будет срабатывать каждую секунду
            _timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            
            // Инициализируем значения
            _balance = 0;
            _remainingTime = TimeSpan.Zero;
            
            Trace.WriteLine($"TimeService создан: {DateTime.Now}");
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    NotifyPropertyChanged(nameof(Balance));
                    Trace.WriteLine($"Balance изменен: {_balance}, время: {DateTime.Now}");
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
                    NotifyPropertyChanged(nameof(RemainingTime));
                    NotifyPropertyChanged(nameof(FormattedRemainingTime));
                    Trace.WriteLine($"RemainingTime изменен: {_remainingTime}, время: {DateTime.Now}");
                }
            }
        }

        public string FormattedRemainingTime
        {
            get
            {
                var time = _remainingTime;
                return time.TotalHours >= 1 
                    ? $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}" 
                    : $"{time.Minutes:D2}:{time.Seconds:D2}";
            }
        }

        public async Task LoadUserDataAsync(int userId)
        {
            try
            {
                // Блокируем параллельные вызовы метода
                await _semaphore.WaitAsync();
                
                _currentUserId = userId;
                Trace.WriteLine($"Загрузка данных пользователя {userId} начата: {DateTime.Now}");
                
                // Получаем данные пользователя
                var userData = await _apiService.GetUserDataAsync(userId);
                
                if (userData != null)
                {
                    Trace.WriteLine($"Данные получены: Баланс={userData.Balance}, Время={userData.RemainingTime}");
                    
                    // Обновляем баланс в UI потоке
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Balance = userData.Balance;
                        RemainingTime = userData.RemainingTime;
                    });
                    
                    // Если у пользователя есть оставшееся время, запускаем таймер
                    if (_remainingTime > TimeSpan.Zero)
                    {
                        StartTimer();
                    }
                    else
                    {
                        StopTimer();
                    }
                    
                    Trace.WriteLine($"Загрузка данных пользователя {userId} завершена: {DateTime.Now}");
                }
                else
                {
                    Trace.WriteLine($"Ошибка: не удалось получить данные пользователя {userId}");
                    MessageBox.Show($"Не удалось загрузить данные пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Исключение при загрузке данных пользователя: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Обертка для обратной совместимости
        public void LoadUserData(int userId)
        {
            _ = LoadUserDataAsync(userId);
        }

        public async Task SaveUserData()
        {
            try
            {
                // Если нет ID пользователя, выходим
                if (!_currentUserId.HasValue)
                {
                    Trace.WriteLine("Нет текущего пользователя для сохранения данных");
                    return;
                }
                
                await _semaphore.WaitAsync();
                
                Trace.WriteLine($"Сохранение данных пользователя {_currentUserId} начато: {DateTime.Now}");
                
                // Отправляем данные на сервер
                var userDataToUpdate = new UserData
                {
                    Balance = _balance,
                    RemainingTime = _remainingTime
                };
                
                bool success = await _apiService.UpdateUserDataAsync(_currentUserId.Value, userDataToUpdate);
                
                if (success)
                {
                    Trace.WriteLine($"Данные пользователя успешно сохранены: {DateTime.Now}");
                }
                else
                {
                    Trace.WriteLine($"Ошибка при сохранении данных пользователя: {DateTime.Now}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Исключение при сохранении данных: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
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
            StartTimer();
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
                
                // Устанавливаем новое значение в потоке UI
                Application.Current.Dispatcher.Invoke(() => {
                    RemainingTime = newRemainingTime;
                });
                
                // Логгируем изменение
                Trace.WriteLine($"Таймер тикает: {FormattedRemainingTime}, время: {DateTime.Now}");
                
                // Каждую минуту сохраняем данные
                if (RemainingTime.Seconds == 0)
                {
                    _ = SaveUserData();
                }
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

        // Отправляет уведомление об изменении свойства в потоке UI
        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            Trace.WriteLine($"NotifyPropertyChanged вызван для {propertyName}: {DateTime.Now}");
            
            // Убедимся, что уведомление отправляется в потоке UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }, DispatcherPriority.Render);
            }
        }
    }
} 