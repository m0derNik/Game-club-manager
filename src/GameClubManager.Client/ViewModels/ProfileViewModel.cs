using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using GameClubManager.Client.Services;
using System.Windows.Input;
using GameClubManager.Client.Commands;

namespace GameClubManager.Client.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private string _username = "Пользователь";
        private string _currentTariff = "Стандартный";
        private BitmapImage _avatarSource;
        private readonly TimeService _timeService;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance => _timeService.Balance;
        public string RemainingTime => _timeService.RemainingTime;

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

        public ICommand AddBalanceCommand { get; }
        public ICommand AddTimeCommand { get; }

        public ProfileViewModel()
        {
            _timeService = TimeService.Instance;
            
            // Подписываемся на изменения времени
            _timeService.PropertyChanged += TimeService_PropertyChanged;

            // Инициализация команд
            AddBalanceCommand = new RelayCommand(_ => AddBalance());
            AddTimeCommand = new RelayCommand(_ => AddTime());
        }

        private void TimeService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeService.RemainingTime))
                OnPropertyChanged(nameof(RemainingTime));
            else if (e.PropertyName == nameof(TimeService.Balance))
                OnPropertyChanged(nameof(Balance));
        }

        private void AddBalance()
        {
            _timeService.AddBalance(100); // Добавляем 100 рублей для теста
        }

        private void AddTime()
        {
            _timeService.AddTime(TimeSpan.FromHours(1)); // Добавляем 1 час для теста
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 