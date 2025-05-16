using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace GameClubManager.Admin.Services
{
    public class BackupService
    {
        private static BackupService _instance;
        private readonly SettingsService _settingsService;
        private readonly ApiService _apiService;
        private DateTime _lastBackupTime;
        private System.Timers.Timer _backupTimer;

        public static BackupService Instance => _instance ??= new BackupService();

        private BackupService()
        {
            _settingsService = SettingsService.Instance;
            _apiService = ApiService.Instance;
            _lastBackupTime = DateTime.Now;
            
            // Настройка таймера для автоматического бэкапа
            _backupTimer = new System.Timers.Timer();
            _backupTimer.Elapsed += async (s, e) => await CheckAndCreateBackupAsync();
            
            // Запускаем таймер
            InitializeBackupTimer();
        }

        private void InitializeBackupTimer()
        {
            var settings = _settingsService.CurrentSettings;
            
            if (settings.AutoBackupEnabled)
            {
                // Проверяем каждые 10 минут, не пора ли сделать бэкап
                _backupTimer.Interval = 10 * 60 * 1000; // 10 минут в миллисекундах
                _backupTimer.Start();
            }
            else
            {
                _backupTimer.Stop();
            }
        }

        private async Task CheckAndCreateBackupAsync()
        {
            var settings = _settingsService.CurrentSettings;
            
            if (!settings.AutoBackupEnabled)
            {
                _backupTimer.Stop();
                return;
            }
            
            var now = DateTime.Now;
            var hoursSinceLastBackup = (now - _lastBackupTime).TotalHours;
            
            if (hoursSinceLastBackup >= settings.BackupIntervalHours)
            {
                await CreateBackupAsync();
            }
        }

        public async Task<bool> CreateBackupAsync()
        {
            try
            {
                // Получаем путь к папке резервных копий
                var backupFolder = _settingsService.GetBackupFolder();
                
                // Создаём имя файла с текущей датой и временем
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"backup_{timestamp}.json";
                var backupFilePath = Path.Combine(backupFolder, backupFileName);
                
                // Здесь мы могли бы запросить данные с сервера для резервного копирования
                // Пока просто создаём пустой файл как пример
                await File.WriteAllTextAsync(backupFilePath, "Резервная копия данных");
                
                _lastBackupTime = DateTime.Now;
                
                return true;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}",
                        "Ошибка резервного копирования", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return false;
            }
        }

        // Обновляем настройки бэкапа при их изменении
        public void UpdateSettings()
        {
            InitializeBackupTimer();
        }
    }
} 