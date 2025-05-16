using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Admin.Models;
using MessageBox = System.Windows.MessageBox;

namespace GameClubManager.Admin.Services
{
    public class TariffService
    {
        private static TariffService _instance;
        private readonly ApiService _apiService;
        private List<Tariff> _tariffs;

        public static TariffService Instance => _instance ??= new TariffService();

        private TariffService()
        {
            _apiService = ApiService.Instance;
            _tariffs = new List<Tariff>();
            LoadTariffsAsync().ConfigureAwait(false);
        }

        public List<Tariff> Tariffs => _tariffs;

        private async Task LoadTariffsAsync()
        {
            try
            {
                var sharedTariffs = await _apiService.GetTariffsAsync();
                _tariffs = ConvertToAdminTariffs(sharedTariffs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке тарифов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public async Task RefreshTariffsAsync()
        {
            await LoadTariffsAsync();
        }

        public async Task<bool> SaveTariffsToFile()
        {
            try
            {
                foreach (var tariff in _tariffs)
                {
                    var sharedTariff = ConvertToSharedTariff(tariff);
                    
                    if (tariff.Id == 0)
                    {
                        // Новый тариф
                        await _apiService.CreateTariffAsync(sharedTariff);
                    }
                    else
                    {
                        // Существующий тариф
                        await _apiService.UpdateTariffAsync(tariff.Id, sharedTariff);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении тарифов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                return false;
            }
        }
        
        public void AddTariff(Tariff tariff)
        {
            // Добавляем только в локальный список, на сервер отправим при сохранении
            _tariffs.Add(tariff);
        }
        
        public void UpdateTariff(Tariff tariff)
        {
            // Обновляем только в локальном списке, на сервер отправим при сохранении
            var index = _tariffs.FindIndex(t => t.Id == tariff.Id);
            if (index >= 0)
            {
                _tariffs[index] = tariff;
            }
        }
        
        public async Task<bool> DeleteTariff(int tariffId)
        {
            try
            {
                var success = await _apiService.DeleteTariffAsync(tariffId);
                if (success)
                {
                    var tariff = _tariffs.Find(t => t.Id == tariffId);
                    if (tariff != null)
                    {
                        _tariffs.Remove(tariff);
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при удалении тарифа: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                return false;
            }
        }
        
        private List<Tariff> ConvertToAdminTariffs(List<GameClubManager.Shared.Models.Tariff> sharedTariffs)
        {
            var adminTariffs = new List<Tariff>();
            foreach (var sharedTariff in sharedTariffs)
            {
                adminTariffs.Add(new Tariff
                {
                    Id = sharedTariff.Id,
                    Name = sharedTariff.Name,
                    Description = sharedTariff.Description,
                    Price = sharedTariff.Price,
                    Duration = sharedTariff.Duration,
                    IsPopular = sharedTariff.IsPopular
                });
            }
            
            return adminTariffs;
        }
        
        private GameClubManager.Shared.Models.Tariff ConvertToSharedTariff(Tariff adminTariff)
        {
            return new GameClubManager.Shared.Models.Tariff
            {
                Id = adminTariff.Id,
                Name = adminTariff.Name,
                Description = adminTariff.Description,
                Price = adminTariff.Price,
                Duration = adminTariff.Duration,
                IsPopular = adminTariff.IsPopular
            };
        }
    }
} 