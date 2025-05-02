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

namespace GameClubManager.Admin.Services
{
    public class ApiService
    {
        private static ApiService? _instance;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:7001/api";
        private string _authToken;

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
            
            // Автоматически выполняем вход администратора при создании экземпляра
            _ = AutoLoginAdminAsync();
        }
        
        private async Task AutoLoginAdminAsync()
        {
            try
            {
                // Используем пустой запрос для автоматического входа
                var request = new LoginRequest
                {
                    Email = "admin",  // Любое значение, так как сервер игнорирует его
                    Password = "admin" // Любое значение, так как сервер игнорирует его
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/admin/auth/login", request);
                
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
                    MessageBox.Show(
                        "Не удалось выполнить автоматический вход в систему. Некоторые функции могут быть недоступны.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
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

                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/admin/auth/login", request);
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
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<List<AdminUser>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/admin/users");
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
                MessageBox.Show($"Ошибка получения списка пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<AdminUser>();
            }
        }

        public async Task<UserData> GetUserDataAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/users/{userId}/data");
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
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/admin/users/{userId}/balance", request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка пополнения баланса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> AddUserTimeAsync(int userId, TimeSpan time)
        {
            try
            {
                var minutes = (int)time.TotalMinutes;
                var request = new AddTimeRequest { Minutes = minutes };
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/admin/users/{userId}/time", request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления времени: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/admin/users/{userId}");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Метод для получения всех заказов
        public async Task<List<OrderResponse>> GetOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/orders");
                response.EnsureSuccessStatusCode();
                var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
                return orders ?? new List<OrderResponse>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<OrderResponse>();
            }
        }

        // Метод для получения заказа по ID
        public async Task<OrderResponse?> GetOrderAsync(int orderId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/orders/{orderId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<OrderResponse>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        endpoint = $"{BaseUrl}/orders/{orderId}/process";
                        break;
                    case OrderStatus.Completed:
                        endpoint = $"{BaseUrl}/orders/{orderId}/complete";
                        break;
                    case OrderStatus.Delivered:
                        endpoint = $"{BaseUrl}/orders/{orderId}/deliver";
                        break;
                    case OrderStatus.Canceled:
                        endpoint = $"{BaseUrl}/orders/{orderId}/cancel";
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
                MessageBox.Show($"Ошибка при обновлении статуса заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

    // Вспомогательные классы для запросов
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
} 