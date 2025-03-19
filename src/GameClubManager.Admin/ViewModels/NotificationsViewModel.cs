using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;

namespace GameClubManager.Admin.ViewModels
{
    public class NotificationsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Notification> _notifications;
        private Notification _selectedNotification;

        public NotificationsViewModel()
        {
            // Временные данные для демонстрации UI
            Notifications = new ObservableCollection<Notification>
            {
                new Notification 
                { 
                    Id = 1, 
                    Title = "Требуется помощь", 
                    Message = "Пользователь на ПК-1 запросил помощь администратора", 
                    Type = "HelpRequest", 
                    Time = DateTime.Now.AddMinutes(-5),
                    IsRead = false
                },
                new Notification 
                { 
                    Id = 2, 
                    Title = "Пополнение баланса", 
                    Message = "Пользователь Иван Иванов пополнил баланс на 500 рублей", 
                    Type = "BalanceUpdate", 
                    Time = DateTime.Now.AddMinutes(-15),
                    IsRead = true
                }
            };

            HandleNotificationCommand = new RelayCommand<Notification>(ExecuteHandleNotification);
            IgnoreNotificationCommand = new RelayCommand<Notification>(ExecuteIgnoreNotification);
        }

        public ObservableCollection<Notification> Notifications
        {
            get => _notifications;
            set
            {
                _notifications = value;
                OnPropertyChanged();
            }
        }

        public Notification SelectedNotification
        {
            get => _selectedNotification;
            set
            {
                _selectedNotification = value;
                OnPropertyChanged();
            }
        }

        public ICommand HandleNotificationCommand { get; }
        public ICommand IgnoreNotificationCommand { get; }

        private void ExecuteHandleNotification(Notification notification)
        {
            // Здесь будет логика обработки уведомления
        }

        private void ExecuteIgnoreNotification(Notification notification)
        {
            // Здесь будет логика игнорирования уведомления
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 