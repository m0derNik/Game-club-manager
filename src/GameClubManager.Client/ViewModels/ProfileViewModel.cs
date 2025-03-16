using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace GameClubManager.Client.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private string _username = "Пользователь";
        private decimal _balance = 1000.00m;
        private TimeSpan _remainingTime = TimeSpan.FromHours(2);
        private string _currentTariff = "Стандартный";
        private BitmapImage _avatarSource;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
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

        public string RemainingTime
        {
            get => $"{_remainingTime.Hours:D2}:{_remainingTime.Minutes:D2}:{_remainingTime.Seconds:D2}";
        }

        public string CurrentTariff
        {
            get => _currentTariff;
            set
            {
                _currentTariff = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage AvatarSource
        {
            get => _avatarSource;
            set
            {
                _avatarSource = value;
                OnPropertyChanged();
            }
        }

        public ProfileViewModel()
        {
            // Здесь можно добавить загрузку данных пользователя
            StartTimer();
        }

        private void StartTimer()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_remainingTime > TimeSpan.Zero)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                OnPropertyChanged(nameof(RemainingTime));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 