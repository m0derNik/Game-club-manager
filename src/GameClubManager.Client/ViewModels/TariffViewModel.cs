using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;
using Timer = System.Threading.Timer;

namespace GameClubManager.Client.ViewModels
{
    public class TariffViewModel : ViewModelBase
    {
        private readonly TimeService _timeService;
        private readonly TariffService _tariffService;
        private ObservableCollection<Tariff> _tariffs;
        private bool _isLoading;
        
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
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand BuyTariffCommand { get; }
        public ICommand RefreshTariffsCommand { get; }
        
        public TariffViewModel()
        {
            _timeService = TimeService.Instance;
            _tariffService = TariffService.Instance;
            _timeService.PropertyChanged += TimeService_PropertyChanged;
            
            BuyTariffCommand = new RelayCommand(BuyTariff);
            RefreshTariffsCommand = new RelayCommand(async _ => await RefreshTariffsAsync());
            
            // Инициализируем набор тарифов
            LoadTariffs();
            
            // Автоматически обновляем тарифы при запуске
            RefreshTariffsOnStartupAsync();
        }
        
        private async void RefreshTariffsOnStartupAsync()
        {
            // Задержка для инициализации UI
            await System.Threading.Tasks.Task.Delay(1000);
            await RefreshTariffsAsync();
        }
        
        private void LoadTariffs()
        {
            // Загружаем тарифы из сервиса
            var serviceTariffs = _tariffService.Tariffs;
            Tariffs = new ObservableCollection<Tariff>(serviceTariffs);
            Console.WriteLine($"Загружено {serviceTariffs.Count} тарифов в UI");
        }
        
        public async System.Threading.Tasks.Task RefreshTariffsAsync()
        {
            try
            {
                Console.WriteLine("Обновление тарифов...");
                IsLoading = true;
                
                // Обновляем тарифы через API
                var success = await _tariffService.RefreshTariffsAsync();
                
                // Обновляем коллекцию
                LoadTariffs();
                
                if (success)
                {
                    Console.WriteLine("Тарифы успешно обновлены");
                }
                else
                {
                    Console.WriteLine("Не удалось обновить тарифы");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении тарифов: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Ошибка при обновлении тарифов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
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
                    
                    System.Windows.MessageBox.Show(
                        $"Вы успешно приобрели тариф \"{tariff.Name}\".\nДобавлено: {FormatDuration(tariff.Duration)}",
                        "Тариф активирован",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show(
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


