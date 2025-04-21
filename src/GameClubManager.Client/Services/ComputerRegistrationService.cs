using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Shared.Models;
using GameClubManager.Client.Models;

namespace GameClubManager.Client.Services
{
    public class ComputerRegistrationService
    {
        private static ComputerRegistrationService? _instance;
        private readonly ApiService _apiService;
        private readonly AuthManager _authManager;

        public static ComputerRegistrationService Instance => _instance ??= new ComputerRegistrationService(ApiService.Instance, AuthManager.Instance);
        
        public ComputerRegistrationService(ApiService apiService, AuthManager authManager)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        public async Task<Models.ComputerDto?> RegisterComputerAsync(
            string name,
            string ipAddress,
            string macAddress,
            string specifications,
            decimal pricePerHour = 200.0m)
        {
            try
            {
                var request = new Models.ComputerRegistrationRequest
                {
                    Name = name,
                    IpAddress = ipAddress,
                    MacAddress = macAddress,
                    Specifications = specifications,
                    PricePerHour = pricePerHour
                };

                return await _apiService.RegisterComputerAsync(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> BindUserToComputerAsync(int computerId, int userId)
        {
            try
            {
                return await _apiService.BindUserToComputerAsync(computerId, userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка привязки пользователя к компьютеру: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UnbindUserFromComputerAsync(int computerId)
        {
            try
            {
                return await _apiService.UnbindUserFromComputerAsync(computerId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отвязки пользователя от компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        public async Task RegisterComputerAsync()
        {
            try
            {
                // Получаем системную информацию
                string computerName = Environment.MachineName;
                string specifications = $"CPU: {Environment.ProcessorCount} cores, OS: {Environment.OSVersion}";
                
                // Регистрируем компьютер
                await RegisterComputerAsync(
                    computerName,
                    "127.0.0.1", // Временно используем локальный IP
                    "00:00:00:00:00:00", // Заглушка для MAC адреса
                    specifications,
                    200.0m); // Цена за час по умолчанию
                
                Trace.WriteLine($"Компьютер успешно зарегистрирован: {computerName}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при регистрации компьютера: {ex.Message}");
            }
        }
        
        public async Task<bool> UpdateStatusOnLoginAsync(int userId)
        {
            try
            {
                // Здесь должна быть логика определения ID текущего компьютера
                int computerId = 1; // Временно используем ID = 1
                
                return await BindUserToComputerAsync(computerId, userId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при обновлении статуса компьютера при входе: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> UpdateStatusOnLogoutAsync()
        {
            try
            {
                // Здесь должна быть логика определения ID текущего компьютера
                int computerId = 1; // Временно используем ID = 1
                
                return await UnbindUserFromComputerAsync(computerId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при обновлении статуса компьютера при выходе: {ex.Message}");
                return false;
            }
        }
    }
}