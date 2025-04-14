using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using GameClubManager.Client.Services;
using System.Windows.Input;
using GameClubManager.Client.Commands;
using System.Threading.Tasks;

namespace GameClubManager.Client.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly AuthManager _authManager;
        private readonly TimeService _timeService;
        private string _username = "Пользователь";
        private string _currentTariff = "Стандартный";
        private BitmapImage _avatarSource;

        public string Username => _authManager.CurrentUser?.Username ?? "Гость";
        
        public decimal Balance
        {
            get => _timeService.Balance;
            set
            {
                if (_timeService.Balance != value)
                {
                    _timeService.Balance = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string RemainingTime
        {
            get => _timeService.FormattedRemainingTime;
            set
            {
                if (_timeService.FormattedRemainingTime != value)
                {
                    OnPropertyChanged();
                }
            }
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

        public ICommand AddBalanceCommand { get; }
        public ICommand LogoutCommand { get; }

        public ProfileViewModel()
        {
            _authManager = AuthManager.Instance;
            _timeService = TimeService.Instance;
            
            // Подписываемся на изменения времени
            _timeService.PropertyChanged += TimeService_PropertyChanged;

            // Инициализация команд
            AddBalanceCommand = new RelayCommand(_ => AddBalance());
            LogoutCommand = new RelayCommand(async _ => await Logout());
        }

        private void TimeService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeService.RemainingTime) || 
                e.PropertyName == nameof(TimeService.FormattedRemainingTime))
            {
                OnPropertyChanged(nameof(RemainingTime));
            }
            else if (e.PropertyName == nameof(TimeService.Balance))
            {
                OnPropertyChanged(nameof(Balance));
            }
        }

        private void AddBalance()
        {
            _timeService.AddBalance(100); // Добавляем 100 рублей для теста
        }

        private async Task Logout()
        {
            await _authManager.Logout();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 