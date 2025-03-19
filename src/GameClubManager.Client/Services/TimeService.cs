using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace GameClubManager.Client.Services
{
    public class TimeService : INotifyPropertyChanged
    {
        private static TimeService _instance;
        private TimeSpan _remainingTime;
        private decimal _balance;
        private DispatcherTimer _timer;

        public static TimeService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TimeService();
                }
                return _instance;
            }
        }

        public string RemainingTime
        {
            get => $"{_remainingTime.Hours:D2}:{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }

        private TimeService()
        {
            _remainingTime = TimeSpan.FromHours(2);
            _balance = 1000.00m;
            StartTimer();
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_remainingTime > TimeSpan.Zero)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                OnPropertyChanged(nameof(RemainingTime));
            }
        }

        public void AddTime(TimeSpan time)
        {
            _remainingTime = _remainingTime.Add(time);
            OnPropertyChanged(nameof(RemainingTime));
        }

        public void AddBalance(decimal amount)
        {
            Balance += amount;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 