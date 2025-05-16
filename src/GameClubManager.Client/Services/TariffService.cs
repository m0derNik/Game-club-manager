using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GameClubManager.Client.Models;
using Timer = System.Threading.Timer;

namespace GameClubManager.Client.Services
{
    public class TariffService
    {
        private static TariffService _instance;
        private readonly HttpClient _httpClient;
        private List<Tariff> _tariffs;
        private string _baseUrl;

        public static TariffService Instance => _instance ??= new TariffService();

        private TariffService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            
            // Загрузка настроек сервера из конфигурации
            var serverAddress = "localhost"; // TODO: загрузить из конфигурации
            var serverPort = 7001; // Правильный порт сервера
            _baseUrl = $"http://{serverAddress}:{serverPort}/api";
            
            _tariffs = new List<Tariff>();
            LoadTariffsAsync().ConfigureAwait(false);
        }

        public List<Tariff> Tariffs => _tariffs;

        private async Task LoadTariffsAsync()
        {
            try
            {
                // Пытаемся загрузить тарифы с API
                var apiTariffs = await GetTariffsFromApiAsync();
                
                if (apiTariffs.Count > 0)
                {
                    _tariffs = apiTariffs;
                }
                else
                {
                    // Если не удалось получить тарифы с API, используем стандартные
                    InitializeDefaultTariffs();
                }
            }
            catch (Exception)
            {
                // В случае ошибки используем стандартные тарифы
                InitializeDefaultTariffs();
            }
        }

        private async Task<List<Tariff>> GetTariffsFromApiAsync()
        {
            try
            {
                Console.WriteLine($"Запрос тарифов с сервера: {_baseUrl}/tariffs");
                var response = await _httpClient.GetAsync($"{_baseUrl}/tariffs");
                
                if (response.IsSuccessStatusCode)
                {
                    var sharedTariffs = await response.Content.ReadFromJsonAsync<List<GameClubManager.Shared.Models.Tariff>>();
                    Console.WriteLine($"Получен ответ от сервера, тарифов: {sharedTariffs?.Count ?? 0}");
                    
                    if (sharedTariffs != null)
                    {
                        // Конвертируем из Shared.Models.Tariff в Client.Models.Tariff
                        var clientTariffs = new List<Tariff>();
                        foreach (var tariff in sharedTariffs)
                        {
                            clientTariffs.Add(new Tariff
                            {
                                Id = tariff.Id,
                                Name = tariff.Name,
                                Description = tariff.Description,
                                Price = tariff.Price,
                                Duration = tariff.Duration,
                                IsPopular = tariff.IsPopular
                            });
                        }
                        
                        Console.WriteLine($"Преобразовано тарифов: {clientTariffs.Count}");
                        return clientTariffs;
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении тарифов: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при загрузке тарифов с API: {ex.Message}");
            }

            return new List<Tariff>();
        }

        // Метод для обновления тарифов с API
        public async Task<bool> RefreshTariffsAsync()
        {
            try
            {
                Console.WriteLine("Обновление тарифов из API...");
                var apiTariffs = await GetTariffsFromApiAsync();
                
                if (apiTariffs.Count > 0)
                {
                    _tariffs = apiTariffs;
                    Console.WriteLine($"Тарифы успешно обновлены из API, количество: {_tariffs.Count}");
                    return true;
                }
                
                Console.WriteLine("Обновление тарифов не удалось - получен пустой список");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении тарифов: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        private void InitializeDefaultTariffs()
        {
            _tariffs = new List<Tariff>
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
    }
} 