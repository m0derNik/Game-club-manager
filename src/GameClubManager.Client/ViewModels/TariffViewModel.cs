using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;

namespace GameClubManager.Client.ViewModels
{
    public class TariffViewModel : ViewModelBase
    {
        private readonly TimeService _timeService;
        private ObservableCollection<Tariff> _tariffs;
        
        public ObservableCollection<Tariff> Tariffs
        {
            get => _tariffs;
            set
            {
                _tariffs = value;
                OnPropertyChanged();
            }
        }
        
        public decimal Balance => _timeService.Balance;
        
        public ICommand BuyTariffCommand { get; }
        
        public TariffViewModel()
        {
            _timeService = TimeService.Instance;
            _timeService.PropertyChanged += TimeService_PropertyChanged;
            
            BuyTariffCommand = new RelayCommand(BuyTariff);
            
            // Инициализируем набор тарифов
            InitializeTariffs();
        }
        
        private void InitializeTariffs()
        {
            Tariffs = new ObservableCollection<Tariff>
            {
                new Tariff 
                { 
                    Id = 1, 
                    Name = "Базовый", 
                    Description = "Доступ к основным играм и сервисам",
                    Price = 100, 
                    Duration = TimeSpan.FromHours(1),
                    IsPopular = false
                },
                new Tariff 
                { 
                    Id = 2, 
                    Name = "Стандартный", 
                    Description = "Стандартный доступ к играм и сервисам",
                    Price = 250, 
                    Duration = TimeSpan.FromHours(3),
                    IsPopular = true
                },
                new Tariff 
                { 
                    Id = 3, 
                    Name = "Продвинутый", 
                    Description = "Расширенный доступ с приоритетным обслуживанием",
                    Price = 400, 
                    Duration = TimeSpan.FromHours(5),
                    IsPopular = false
                },
                new Tariff 
                { 
                    Id = 4, 
                    Name = "Ночной", 
                    Description = "Тариф для ночных игровых сессий",
                    Price = 500, 
                    Duration = TimeSpan.FromHours(8),
                    IsPopular = false
                },
                new Tariff 
                { 
                    Id = 5, 
                    Name = "Турнирный", 
                    Description = "Специальный тариф для игроков в турнирах",
                    Price = 1000, 
                    Duration = TimeSpan.FromHours(24),
                    IsPopular = false
                }
            };
        }
        
        private void BuyTariff(object parameter)
        {
            if (parameter is Tariff tariff)
            {
                if (_timeService.Balance >= tariff.Price)
                {
                    // Вычитаем стоимость тарифа из баланса
                    _timeService.Balance -= tariff.Price;
                    
                    // Добавляем время
                    _timeService.AddTime(tariff.Duration);
                    
                    MessageBox.Show(
                        $"Вы успешно приобрели тариф \"{tariff.Name}\".\nДобавлено: {FormatDuration(tariff.Duration)}",
                        "Тариф активирован",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Недостаточно средств для покупки тарифа.\nНеобходимо: {tariff.Price:C}, доступно: {_timeService.Balance:C}",
                        "Ошибка покупки",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }
        
        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours} ч {duration.Minutes} мин";
            }
            return $"{duration.Minutes} мин";
        }
        
        private void TimeService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeService.Balance))
            {
                OnPropertyChanged(nameof(Balance));
            }
        }
    }
} 