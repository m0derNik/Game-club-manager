using System.ComponentModel;
using System.Runtime.CompilerServices;
using GameClubManager.Client.Services;
using GameClubManager.Client.Commands;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;

namespace GameClubManager.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly AuthManager _authManager;
        private readonly TimeService _timeService;
        private string _title = "Game Club Manager";
        private string _formattedRemainingTime;
        private decimal _balance;
        private readonly DispatcherTimer _updateTimer;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance
        {
            get => _balance;
            private set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged();
                    Trace.WriteLine($"Balance изменен в ViewModel: {_balance}");
                }
            }
        }

        public string RemainingTime
        {
            get => _formattedRemainingTime;
            private set
            {
                if (_formattedRemainingTime != value)
                {
                    _formattedRemainingTime = value;
                    OnPropertyChanged();
                    Trace.WriteLine($"RemainingTime изменен в ViewModel: {_formattedRemainingTime}");
                }
            }
        }

        public string Username => _authManager.CurrentUser?.Username ?? "Гость";

        public MainWindowViewModel()
        {
            _authManager = AuthManager.Instance;
            _timeService = TimeService.Instance;
            
            // Создаем дополнительный таймер для принудительного обновления UI
            _updateTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
            
            // Инициализация данных
            UpdateDataFromService();
            
            // Подписываемся на изменения времени и баланса
            _timeService.PropertyChanged += TimeService_PropertyChanged;
            
            Trace.WriteLine($"MainWindowViewModel создан: Баланс={Balance}, Время={RemainingTime}");
        }
        
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Принудительно обновляем данные каждые 500 мс
            Application.Current.Dispatcher.Invoke(() => {
                var currentTime = _timeService.FormattedRemainingTime;
                if (_formattedRemainingTime != currentTime)
                {
                    RemainingTime = currentTime;
                    Trace.WriteLine($"Принудительное обновление времени в ViewModel: {RemainingTime}");
                }
            }, DispatcherPriority.Render);
        }

        public void UpdateDataFromService()
        {
            Balance = _timeService.Balance;
            RemainingTime = _timeService.FormattedRemainingTime;
            
            Trace.WriteLine($"Данные обновлены в ViewModel: Баланс={Balance}, Время={RemainingTime}");
        }

        private void TimeService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Выполняем обновление в UI потоке
            Application.Current.Dispatcher.Invoke(() => {
                if (e.PropertyName == nameof(TimeService.FormattedRemainingTime) || 
                    e.PropertyName == nameof(TimeService.RemainingTime))
                {
                    RemainingTime = _timeService.FormattedRemainingTime;
                    Trace.WriteLine($"Обновление из TimeService - Время: {RemainingTime}");
                }
                else if (e.PropertyName == nameof(TimeService.Balance))
                {
                    Balance = _timeService.Balance;
                    Trace.WriteLine($"Обновление из TimeService - Баланс: {Balance}");
                }
            }, DispatcherPriority.Render);
        }
        
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Trace.WriteLine($"ViewModel.OnPropertyChanged вызван для {propertyName}: {DateTime.Now}");
            base.OnPropertyChanged(propertyName);
        }
    }
} 