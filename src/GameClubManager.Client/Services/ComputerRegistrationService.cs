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
using System.Net.NetworkInformation;
using System.Linq;
using System.Net;

namespace GameClubManager.Client.Services
{
    public class ComputerRegistrationService
    {
        private static ComputerRegistrationService? _instance;
        private readonly ApiService _apiService;
        private readonly AuthManager _authManager;
        private int? _cachedComputerId;

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

                var result = await _apiService.RegisterComputerAsync(request);
                if (result != null)
                {
                    _cachedComputerId = result.Id;
                }
                return result;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка регистрации компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show($"Ошибка привязки пользователя к компьютеру: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show($"Ошибка отвязки пользователя от компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
                // Получаем IP и MAC адреса
                var (ipAddress, macAddress) = GetNetworkInfo();
                
                // Регистрируем компьютер
                await RegisterComputerAsync(
                    computerName,
                    ipAddress,
                    macAddress,
                    specifications,
                    200.0m); // Цена за час по умолчанию
                
                Trace.WriteLine($"Компьютер успешно зарегистрирован: {computerName}, IP: {ipAddress}, MAC: {macAddress}");
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
                int computerId = await GetCurrentComputerIdAsync();
                if (computerId == 0)
                {
                    Trace.WriteLine("Не удалось определить ID компьютера для обновления статуса.");
                    return false;
                }
                
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
                int computerId = await GetCurrentComputerIdAsync();
                if (computerId == 0)
                {
                    Trace.WriteLine("Не удалось определить ID компьютера для обновления статуса.");
                    return false;
                }
                
                return await UnbindUserFromComputerAsync(computerId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при обновлении статуса компьютера при выходе: {ex.Message}");
                return false;
            }
        }
        
        public async Task<int> GetCurrentComputerIdAsync()
        {
            // Если ID уже был получен ранее, возвращаем его из кэша
            if (_cachedComputerId.HasValue)
            {
                return _cachedComputerId.Value;
            }
            
            try
            {
                // Получаем текущий IP и MAC адрес
                var (ipAddress, macAddress) = GetNetworkInfo();
                
                // Получаем список всех компьютеров
                var computers = await _apiService.GetComputersAsync();
                if (computers == null || !computers.Any())
                {
                    Trace.WriteLine("Список компьютеров пуст. Возможно, компьютер еще не зарегистрирован.");
                    return 0;
                }
                
                // Ищем компьютер по MAC-адресу и IP-адресу
                var computer = computers.FirstOrDefault(c => 
                    c.MacAddress.Equals(macAddress, StringComparison.OrdinalIgnoreCase) && 
                    c.IpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase));
                
                if (computer != null)
                {
                    _cachedComputerId = computer.Id;
                    return computer.Id;
                }
                
                Trace.WriteLine($"Компьютер с MAC-адресом {macAddress} и IP-адресом {ipAddress} не найден в базе данных.");
                return 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при получении ID компьютера: {ex.Message}");
                return 0;
            }
        }
        
        private (string ipAddress, string macAddress) GetNetworkInfo()
        {
            string ipAddress = "127.0.0.1";
            string macAddress = "00:00:00:00:00:00";
            
            try
            {
                // Получаем информацию о сетевых интерфейсах
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .ToList();
                
                if (networkInterfaces.Any())
                {
                    // Берем первый активный интерфейс
                    var firstInterface = networkInterfaces.First();
                    
                    // Получаем MAC-адрес
                    var physicalAddress = firstInterface.GetPhysicalAddress();
                    if (physicalAddress != null)
                    {
                        byte[] bytes = physicalAddress.GetAddressBytes();
                        macAddress = string.Join(":", bytes.Select(b => b.ToString("X2")));
                    }
                    
                    // Получаем IP-адрес
                    var ipProps = firstInterface.GetIPProperties();
                    var ipAddresses = ipProps.UnicastAddresses
                        .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        .Select(addr => addr.Address.ToString())
                        .ToList();
                    
                    if (ipAddresses.Any())
                    {
                        ipAddress = ipAddresses.First();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Ошибка при получении сетевой информации: {ex.Message}");
                // В случае ошибки возвращаем стандартные значения
            }
            
            return (ipAddress, macAddress);
        }
    }
}


