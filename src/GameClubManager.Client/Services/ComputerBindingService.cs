using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Client.Models;

namespace GameClubManager.Client.Services
{
    public class ComputerBindingService
    {
        private static ComputerBindingService _instance;
        private readonly ApiService _apiService;
        private readonly AuthManager _authManager;
        
        public static ComputerBindingService Instance => _instance ??= new ComputerBindingService();
        
        private ComputerBindingService()
        {
            _apiService = ApiService.Instance;
            _authManager = AuthManager.Instance;
        }
        
        // Метод для получения списка доступных компьютеров
        public async Task<List<Models.ComputerDto>> GetAvailableComputersAsync()
        {
            try
            {
                return await _apiService.GetAvailableComputersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении списка компьютеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Models.ComputerDto>();
            }
        }
        
        // Метод для привязки пользователя к компьютеру
        public async Task<bool> BindUserToComputer(int computerId)
        {
            try
            {
                if (!_authManager.IsAuthenticated)
                {
                    MessageBox.Show("Необходимо авторизоваться для выбора компьютера", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                
                var result = await _apiService.BindUserToComputerAsync(computerId, _authManager.CurrentUserId);
                if (result)
                {
                    try
                    {
                        // Получаем информацию о выбранном компьютере
                        var computer = await _apiService.GetComputerAsync(computerId);
                        if (computer != null)
                        {
                            // Обновляем текущий компьютер в AuthManager
                            _authManager.CurrentComputer = computer;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Компьютер привязан, но не удалось получить информацию о нем: {ex.Message}", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return true; // Привязка все равно выполнена успешно
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при привязке к компьютеру: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Метод для отвязки пользователя от компьютера
        public async Task<bool> UnbindUserFromComputer()
        {
            try
            {
                if (!_authManager.IsAuthenticated)
                {
                    MessageBox.Show("Необходимо авторизоваться", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                
                if (_authManager.CurrentComputer == null)
                {
                    MessageBox.Show("Нет привязанного компьютера", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                
                var result = await _apiService.UnbindUserFromComputerAsync(_authManager.CurrentComputer.Id);
                if (result)
                {
                    // Сбрасываем текущий компьютер в AuthManager
                    _authManager.CurrentComputer = null;
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отвязке от компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
} 