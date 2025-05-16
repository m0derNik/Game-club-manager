using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

namespace GameClubManager.Admin.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private Settings _settings;
        private bool _isSaving;

        public SettingsViewModel()
        {
            _settingsService = SettingsService.Instance;
            _settings = new Settings();
            
            // Загружаем настройки
            LoadSettings();

            // Инициализируем команды
            SaveSettingsCommand = new RelayCommand(ExecuteSaveSettings, CanExecuteSaveSettings);
            ResetSettingsCommand = new RelayCommand(ExecuteResetSettings);
            BrowseBackupPathCommand = new RelayCommand(ExecuteBrowseBackupPath);
        }

        private void LoadSettings()
        {
            var currentSettings = _settingsService.CurrentSettings;
            
            // Копируем значения в локальные свойства
            ClubName = currentSettings.ClubName;
            Address = currentSettings.Address;
            Phone = currentSettings.Phone;
            HourlyRate = currentSettings.HourlyRate;
            MinimumTime = currentSettings.MinimumTime;
            MaximumTime = currentSettings.MaximumTime;
            SoundNotificationsEnabled = currentSettings.SoundNotificationsEnabled;
            PopupNotificationsEnabled = currentSettings.PopupNotificationsEnabled;
            LowBalanceNotificationsEnabled = currentSettings.LowBalanceNotificationsEnabled;
            ServerAddress = currentSettings.ServerAddress;
            ServerPort = currentSettings.ServerPort;
            AutoBackupEnabled = currentSettings.AutoBackupEnabled;
            BackupIntervalHours = currentSettings.BackupIntervalHours;
            BackupPath = currentSettings.BackupPath;
        }

        #region Свойства

        // Общие настройки
        private string _clubName;
        public string ClubName
        {
            get => _clubName;
            set
            {
                _clubName = value;
                OnPropertyChanged();
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        // Настройки тарифов
        private decimal _hourlyRate;
        public decimal HourlyRate
        {
            get => _hourlyRate;
            set
            {
                _hourlyRate = value;
                OnPropertyChanged();
            }
        }

        private int _minimumTime;
        public int MinimumTime
        {
            get => _minimumTime;
            set
            {
                _minimumTime = value;
                OnPropertyChanged();
            }
        }

        private int _maximumTime;
        public int MaximumTime
        {
            get => _maximumTime;
            set
            {
                _maximumTime = value;
                OnPropertyChanged();
            }
        }

        // Настройки уведомлений
        private bool _soundNotificationsEnabled;
        public bool SoundNotificationsEnabled
        {
            get => _soundNotificationsEnabled;
            set
            {
                _soundNotificationsEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _popupNotificationsEnabled;
        public bool PopupNotificationsEnabled
        {
            get => _popupNotificationsEnabled;
            set
            {
                _popupNotificationsEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _lowBalanceNotificationsEnabled;
        public bool LowBalanceNotificationsEnabled
        {
            get => _lowBalanceNotificationsEnabled;
            set
            {
                _lowBalanceNotificationsEnabled = value;
                OnPropertyChanged();
            }
        }

        // Настройки сервера
        private string _serverAddress;
        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                _serverAddress = value;
                OnPropertyChanged();
            }
        }

        private int _serverPort;
        public int ServerPort
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                OnPropertyChanged();
            }
        }

        // Настройки резервного копирования
        private bool _autoBackupEnabled;
        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set
            {
                _autoBackupEnabled = value;
                OnPropertyChanged();
            }
        }

        private int _backupIntervalHours;
        public int BackupIntervalHours
        {
            get => _backupIntervalHours;
            set
            {
                _backupIntervalHours = value;
                OnPropertyChanged();
            }
        }

        private string _backupPath;
        public string BackupPath
        {
            get => _backupPath;
            set
            {
                _backupPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged();
                (SaveSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Команды

        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetSettingsCommand { get; }
        public ICommand BrowseBackupPathCommand { get; }

        private async void ExecuteSaveSettings()
        {
            try
            {
                IsSaving = true;
                
                // Создаем новый объект настроек
                var settings = new Settings
                {
                    ClubName = ClubName,
                    Address = Address,
                    Phone = Phone,
                    HourlyRate = HourlyRate,
                    MinimumTime = MinimumTime,
                    MaximumTime = MaximumTime,
                    SoundNotificationsEnabled = SoundNotificationsEnabled,
                    PopupNotificationsEnabled = PopupNotificationsEnabled,
                    LowBalanceNotificationsEnabled = LowBalanceNotificationsEnabled,
                    ServerAddress = ServerAddress,
                    ServerPort = ServerPort,
                    AutoBackupEnabled = AutoBackupEnabled,
                    BackupIntervalHours = BackupIntervalHours,
                    BackupPath = BackupPath
                };
                
                // Сохраняем настройки
                bool success = await _settingsService.SaveSettingsAsync(settings);
                
                if (success)
                {
                    // Обновляем настройки резервного копирования
                    BackupService.Instance.UpdateSettings();
                    
                    MessageBox.Show("Настройки успешно сохранены", "Сохранение настроек", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool CanExecuteSaveSettings() => !IsSaving;

        private async void ExecuteResetSettings()
        {
            if (MessageBox.Show("Вы уверены, что хотите сбросить все настройки до значений по умолчанию?", 
                "Сброс настроек", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _settingsService.ResetSettingsAsync();
                LoadSettings();
                MessageBox.Show("Настройки сброшены до значений по умолчанию", "Сброс настроек", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteBrowseBackupPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Выберите папку для резервных копий",
                ShowNewFolderButton = true
            };
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BackupPath = dialog.SelectedPath;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 