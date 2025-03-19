using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Admin.Commands;

namespace GameClubManager.Admin.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _clubName;
        private string _address;
        private string _phone;
        private decimal _hourlyRate;
        private int _minimumTime;
        private int _maximumTime;
        private bool _soundNotificationsEnabled;
        private bool _popupNotificationsEnabled;
        private bool _lowBalanceNotificationsEnabled;

        public SettingsViewModel()
        {
            // Временные данные для демонстрации UI
            ClubName = "Game Club";
            Address = "ул. Примерная, 123";
            Phone = "+7 (999) 123-45-67";
            HourlyRate = 100;
            MinimumTime = 30;
            MaximumTime = 24;
            SoundNotificationsEnabled = true;
            PopupNotificationsEnabled = true;
            LowBalanceNotificationsEnabled = true;

            SaveSettingsCommand = new RelayCommand(ExecuteSaveSettings);
            ResetSettingsCommand = new RelayCommand(ExecuteResetSettings);
        }

        public string ClubName
        {
            get => _clubName;
            set
            {
                _clubName = value;
                OnPropertyChanged();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public decimal HourlyRate
        {
            get => _hourlyRate;
            set
            {
                _hourlyRate = value;
                OnPropertyChanged();
            }
        }

        public int MinimumTime
        {
            get => _minimumTime;
            set
            {
                _minimumTime = value;
                OnPropertyChanged();
            }
        }

        public int MaximumTime
        {
            get => _maximumTime;
            set
            {
                _maximumTime = value;
                OnPropertyChanged();
            }
        }

        public bool SoundNotificationsEnabled
        {
            get => _soundNotificationsEnabled;
            set
            {
                _soundNotificationsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool PopupNotificationsEnabled
        {
            get => _popupNotificationsEnabled;
            set
            {
                _popupNotificationsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool LowBalanceNotificationsEnabled
        {
            get => _lowBalanceNotificationsEnabled;
            set
            {
                _lowBalanceNotificationsEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetSettingsCommand { get; }

        private void ExecuteSaveSettings()
        {
            // Здесь будет логика сохранения настроек
        }

        private void ExecuteResetSettings()
        {
            // Здесь будет логика сброса настроек
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 