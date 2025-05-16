using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
// Используем псевдоним для Admin.Models.User, чтобы избежать конфликта
using AdminUser = GameClubManager.Admin.Models.User;
using GameClubManager.Shared.Models;
using AdminFoodItem = GameClubManager.Admin.Models.FoodItem;
using SharedFoodItem = GameClubManager.Shared.Models.FoodItem;
using AdminGame = GameClubManager.Admin.Models.Game;
using SharedGame = GameClubManager.Shared.Models.Game;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GameClubManager.Admin.Services
{
    public class ApiService
    {
        private static ApiService? _instance;
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private string _authToken;
        private readonly SettingsService _settingsService;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static ApiService Instance => _instance ??= new ApiService();

        private ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            
            _settingsService = SettingsService.Instance;
            // Инициализация логгера
            _logger = new EmptyLogger<ApiService>();
            UpdateServerSettings();
            
            // Устанавливаем специальный заголовок для идентификации приложения админа
            _httpClient.DefaultRequestHeaders.Add("X-Admin-Client", "true");
            
            // Автоматически выполняем вход администратора при создании экземпляра
            _ = AutoLoginAdminAsync();
        }
        
        private void UpdateServerSettings()
        {
            var settings = _settingsService.CurrentSettings;
            _baseUrl = $"http://{settings.ServerAddress}:{settings.ServerPort}/api";
        }
        
        private async Task AutoLoginAdminAsync()
        {
            try
            {
                // Используем специальный эндпоинт, который автоматически аутентифицирует админское приложение
                var response = await _httpClient.PostAsync($"{_baseUrl}/admin/auth/auto-login", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (authResponse != null)
                    {
                        SetAuthToken(authResponse.Token);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Не удалось выполнить автоматический вход в систему. Некоторые функции могут быть недоступны.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка при автоматическом входе: {ex.Message}. Проверьте подключение к серверу.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void SetAuthToken(string? token)
        {
            _authToken = token;
            if (string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<bool> AdminLoginAsync(string username, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Email = username,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/admin/auth/login", request);
                response.EnsureSuccessStatusCode();
                
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null && authResponse.User.Role == UserRole.Admin)
                {
                    SetAuthToken(authResponse.Token);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<List<AdminUser>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/admin/users");
                response.EnsureSuccessStatusCode();
                
                var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                if (users == null) return new List<AdminUser>();

                var result = new List<AdminUser>();
                foreach (var user in users)
                {
                    var userData = await GetUserDataAsync(user.Id);
                    result.Add(new AdminUser
                    {
                        Id = user.Id,
                        Name = user.Username,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        Balance = user.Balance,
                        RemainingTime = userData?.RemainingTime ?? TimeSpan.Zero,
                        Status = "Активен", // TODO: получать статус с сервера
                        RegistrationDate = DateTime.Now.AddDays(-30) // TODO: получать дату с сервера
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка получения списка пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<AdminUser>();
            }
        }

        public async Task<UserData> GetUserDataAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}/data");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserData>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AddUserBalanceAsync(int userId, decimal amount)
        {
            try
            {
                var request = new AddBalanceRequest { Amount = amount };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/admin/users/{userId}/balance", request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка пополнения баланса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> AddUserTimeAsync(int userId, TimeSpan time)
        {
            try
            {
                var minutes = (int)time.TotalMinutes;
                var request = new AddTimeRequest { Minutes = minutes };
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/admin/users/{userId}/time", request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка добавления времени: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/admin/users/{userId}");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Метод для получения всех заказов
        public async Task<List<OrderResponse>> GetOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/orders");
                response.EnsureSuccessStatusCode();
                var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
                return orders ?? new List<OrderResponse>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<OrderResponse>();
            }
        }

        // Метод для получения заказа по ID
        public async Task<OrderResponse?> GetOrderAsync(int orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/orders/{orderId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<OrderResponse>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // Методы для обновления статуса заказа
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            try
            {
                string endpoint;
                
                switch (status)
                {
                    case OrderStatus.Processing:
                        endpoint = $"{_baseUrl}/orders/{orderId}/process";
                        break;
                    case OrderStatus.Completed:
                        endpoint = $"{_baseUrl}/orders/{orderId}/complete";
                        break;
                    case OrderStatus.Delivered:
                        endpoint = $"{_baseUrl}/orders/{orderId}/deliver";
                        break;
                    case OrderStatus.Canceled:
                        endpoint = $"{_baseUrl}/orders/{orderId}/cancel";
                        break;
                    default:
                        throw new ArgumentException($"Неподдерживаемый статус заказа: {status}");
                }
                
                var response = await _httpClient.PostAsync(endpoint, null);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при обновлении статуса заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Методы для работы с уведомлениями
        public async Task<List<AdminNotification>> GetNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/notifications");
                response.EnsureSuccessStatusCode();
                
                var serverNotifications = await response.Content.ReadFromJsonAsync<List<AdminNotification>>();
                return serverNotifications ?? new List<AdminNotification>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении уведомлений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<AdminNotification>();
            }
        }
        
        public async Task<List<AdminNotification>> GetUnreadNotificationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/notifications/unread");
                response.EnsureSuccessStatusCode();
                
                var serverNotifications = await response.Content.ReadFromJsonAsync<List<AdminNotification>>();
                return serverNotifications ?? new List<AdminNotification>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении непрочитанных уведомлений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<AdminNotification>();
            }
        }
        
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/notifications/{notificationId}/read", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при отметке уведомления как прочитанного: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/admin/notifications/{notificationId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при удалении уведомления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Методы для работы с тарифами
        public async Task<List<GameClubManager.Shared.Models.Tariff>> GetTariffsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/tariffs");
                response.EnsureSuccessStatusCode();
                
                var tariffs = await response.Content.ReadFromJsonAsync<List<GameClubManager.Shared.Models.Tariff>>();
                return tariffs ?? new List<GameClubManager.Shared.Models.Tariff>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка получения списка тарифов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<GameClubManager.Shared.Models.Tariff>();
            }
        }
        
        public async Task<GameClubManager.Shared.Models.Tariff> GetTariffAsync(int tariffId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/tariffs/{tariffId}");
                response.EnsureSuccessStatusCode();
                
                var tariff = await response.Content.ReadFromJsonAsync<GameClubManager.Shared.Models.Tariff>();
                return tariff;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка получения тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        
        public async Task<GameClubManager.Shared.Models.Tariff> CreateTariffAsync(GameClubManager.Shared.Models.Tariff tariff)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/tariffs", tariff);
                response.EnsureSuccessStatusCode();
                
                var createdTariff = await response.Content.ReadFromJsonAsync<GameClubManager.Shared.Models.Tariff>();
                return createdTariff;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка создания тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        
        public async Task<bool> UpdateTariffAsync(int tariffId, GameClubManager.Shared.Models.Tariff tariff)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/tariffs/{tariffId}", tariff);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка обновления тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        public async Task<bool> DeleteTariffAsync(int tariffId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/tariffs/{tariffId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка удаления тарифа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<List<SharedFoodItem>> GetFoodItemsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/food");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<SharedFoodItem>>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка получения списка продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<SharedFoodItem>();
            }
        }

        public async Task<SharedFoodItem> GetFoodItemAsync(int foodItemId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/food/{foodItemId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SharedFoodItem>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка получения продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<SharedFoodItem> CreateFoodItemAsync(SharedFoodItem foodItem)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/food", foodItem);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SharedFoodItem>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка создания продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> UpdateFoodItemAsync(int foodItemId, SharedFoodItem foodItem)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/food/{foodItemId}", foodItem);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка обновления продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteFoodItemAsync(int foodItemId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/food/{foodItemId}");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка удаления продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Методы для работы с играми
        
        public async Task<List<SharedGame>> GetGamesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/games");
                await EnsureSuccessStatusCode(response);
                
                var gamesJson = await response.Content.ReadAsStringAsync();
                var games = JsonSerializer.Deserialize<List<SharedGame>>(gamesJson, _jsonOptions);
                
                return games ?? new List<SharedGame>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка игр");
                throw;
            }
        }
        
        public async Task<SharedGame> GetGameAsync(int gameId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/games/{gameId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<SharedGame>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка получения информации об игре: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        
        public async Task<SharedGame> AddGameAsync(SharedGame game)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(game, _jsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/games", content);
                await EnsureSuccessStatusCode(response);
                
                var gameJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SharedGame>(gameJson, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении игры");
                throw;
            }
        }
        
        public async Task<bool> UpdateGameAsync(SharedGame game)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(game, _jsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/games/{game.Id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении игры с ID {GameId}", game.Id);
                throw;
            }
        }
        
        public async Task<bool> DeleteGameAsync(int gameId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/games/{gameId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении игры с ID {GameId}", gameId);
                throw;
            }
        }
        
        public async Task<Models.GameAvailabilityResponse> ToggleGameAvailabilityAsync(int gameId)
        {
            try
            {
                // Логируем отправку запроса
                _logger.LogInformation("Отправка запроса на изменение доступности игры с ID {GameId}", gameId);
                
                // Выводим информацию о текущих заголовках
                _logger.LogInformation("Текущие заголовки авторизации: {Headers}", 
                    _httpClient.DefaultRequestHeaders.Authorization?.ToString() ?? "отсутствуют");
                _logger.LogInformation("Наличие заголовка X-Admin-Client: {HasHeader}", 
                    _httpClient.DefaultRequestHeaders.Contains("X-Admin-Client"));
                
                var response = await _httpClient.PutAsync($"{_baseUrl}/games/{gameId}/toggle-availability", null);
                
                // Логируем результат запроса
                _logger.LogInformation("Результат запроса: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GameClubManager.Shared.Models.GameAvailabilityResponse>(_jsonOptions);
                    
                    // Преобразуем ответ от сервера в модель клиента
                    if (result != null)
                    {
                        return new Models.GameAvailabilityResponse
                        {
                            GameId = result.GameId,
                            IsAvailable = result.IsAvailable,
                            UpdatedAt = result.UpdatedAt
                        };
                    }
                }
                else
                {
                    // Логируем содержимое ответа в случае ошибки
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Ошибка при изменении доступности игры: {Error}", content);
                    
                    // Показываем ошибку пользователю
                    System.Windows.MessageBox.Show(
                        $"Ошибка при изменении доступности игры (код {(int)response.StatusCode}): {content}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении доступности игры с ID {GameId}", gameId);
                throw;
            }
        }
        
        // Вспомогательные классы
        
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AddBalanceRequest
        {
            public decimal Amount { get; set; }
        }

        public class AddTimeRequest
        {
            public int Minutes { get; set; }
        }

        public class UserData
        {
            public decimal Balance { get; set; }
            public TimeSpan RemainingTime { get; set; }
        }

        public class ToggleAvailabilityResponse
        {
            public int Id { get; set; }
            public bool IsAvailable { get; set; }
        }

        private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"API request failed with status code: {response.StatusCode}. Response content: {errorMessage}");
            }
        }
    }
} 