using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Admin.Models;
using MessageBox = System.Windows.MessageBox;

namespace GameClubManager.Admin.Services
{
    public class SettingsService
    {
        private static SettingsService _instance;
        private readonly string _settingsFilePath;
        private Settings _currentSettings;

        public static SettingsService Instance => _instance ??= new SettingsService();

        private SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var gameclubPath = Path.Combine(appDataPath, "GameClubManager");
            
            if (!Directory.Exists(gameclubPath))
            {
                Directory.CreateDirectory(gameclubPath);
            }
            
            _settingsFilePath = Path.Combine(gameclubPath, "settings.json");
            _currentSettings = LoadSettings();
        }

        public Settings CurrentSettings => _currentSettings;

        private Settings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    return settings ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке настроек: {ex.Message}. Будут использованы настройки по умолчанию.",
                    "Ошибка загрузки настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return new Settings();
        }

        public async Task<bool> SaveSettingsAsync(Settings settings)
        {
            try
            {
                _currentSettings = settings;
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);
                
                await File.WriteAllTextAsync(_settingsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}",
                    "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> ResetSettingsAsync()
        {
            _currentSettings = new Settings();
            return await SaveSettingsAsync(_currentSettings);
        }
        
        public string GetBackupFolder()
        {
            if (!Directory.Exists(_currentSettings.BackupPath))
            {
                try
                {
                    Directory.CreateDirectory(_currentSettings.BackupPath);
                }
                catch
                {
                    _currentSettings.BackupPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }
            
            return _currentSettings.BackupPath;
        }
    }
} 