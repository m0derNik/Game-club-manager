using System;

namespace GameClubManager.Admin.Models
{
    public class Settings
    {
        // Общие настройки
        public string ClubName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        
        // Настройки тарифов
        public decimal HourlyRate { get; set; }
        public int MinimumTime { get; set; }
        public int MaximumTime { get; set; }
        
        // Настройки уведомлений
        public bool SoundNotificationsEnabled { get; set; }
        public bool PopupNotificationsEnabled { get; set; }
        public bool LowBalanceNotificationsEnabled { get; set; }
        
        // Настройки сервера
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        
        // Настройки резервного копирования
        public bool AutoBackupEnabled { get; set; }
        public int BackupIntervalHours { get; set; }
        public string BackupPath { get; set; }
        
        // Конструктор с настройками по умолчанию
        public Settings()
        {
            ClubName = "Game Club";
            Address = "ул. Примерная, 123";
            Phone = "+7 (999) 123-45-67";
            HourlyRate = 100;
            MinimumTime = 30;
            MaximumTime = 24;
            SoundNotificationsEnabled = true;
            PopupNotificationsEnabled = true;
            LowBalanceNotificationsEnabled = true;
            ServerAddress = "localhost";
            ServerPort = 7001;
            AutoBackupEnabled = false;
            BackupIntervalHours = 24;
            BackupPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GameClubBackups";
        }
    }
} 