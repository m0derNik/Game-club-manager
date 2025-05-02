using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;
using System.Windows;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GameClubManager.Client.Models;
using ClientComputerStatus = GameClubManager.Client.Models.ComputerStatus;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameClubManager.Client.Services;

public class ApiService
{
    private static ApiService? _instance;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:7001/api";
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

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
    }

    public void SetAuthToken(string? token)
    {
        // Временно отключаем аутентификацию
        // if (string.IsNullOrEmpty(token))
        // {
        //     _httpClient.DefaultRequestHeaders.Remove("Authorization");
        //     MessageBox.Show("Токен удален из заголовков", "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);
        // }
        // else
        // {
        //     _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //     MessageBox.Show($"Токен установлен: {token}", "Отладка", MessageBoxButton.OK, MessageBoxImage.Information);
        // }
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public async Task<UserData?> GetUserDataAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/users/{userId}/data");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserData>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    public async Task<bool> UpdateUserDataAsync(int userId, UserData userData)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/users/{userId}/data", userData);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка обновления данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public async Task<Models.ComputerDto?> RegisterComputerAsync(Models.ComputerRegistrationRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/computers/register", request);
            response.EnsureSuccessStatusCode();
            
            // Читаем ответ напрямую как объект ComputerDto сервера
            var serverDto = await response.Content.ReadFromJsonAsync<ServerComputerDto>();
            if (serverDto == null) return null;
            
            // Преобразуем в наш локальный DTO
            return new Models.ComputerDto
            {
                Id = serverDto.Id,
                Name = serverDto.Name,
                Specifications = serverDto.Specifications,
                Status = Models.ComputerStatus.Available,
                CurrentUserId = serverDto.CurrentUserId,
                PricePerHour = serverDto.PricePerHour
            };
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
            var request = new ComputerStatusUpdateRequest
            {
                Status = Models.ComputerStatus.Occupied,
                UserId = userId
            };

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/computers/{computerId}/status", request);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при привязке пользователя к компьютеру: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public async Task<bool> UnbindUserFromComputerAsync(int computerId)
    {
        try
        {
            var request = new ComputerStatusUpdateRequest
            {
                Status = Models.ComputerStatus.Available,
                UserId = null
            };

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/computers/{computerId}/status", request);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при отвязке пользователя от компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public async Task<List<Models.ComputerDto>> GetComputersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/computers");
            response.EnsureSuccessStatusCode();
            
            var serverDtos = await response.Content.ReadFromJsonAsync<List<ServerComputerDto>>();
            if (serverDtos == null) return new List<Models.ComputerDto>();
            
            return serverDtos.Select(dto => new Models.ComputerDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Specifications = dto.Specifications,
                Status = Enum.Parse<Models.ComputerStatus>(dto.Status, true),
                CurrentUserId = dto.CurrentUserId,
                PricePerHour = dto.PricePerHour
            }).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка получения списка компьютеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Models.ComputerDto>();
        }
    }

    public async Task<Models.ComputerDto?> GetComputerAsync(int computerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/computers/{computerId}");
            response.EnsureSuccessStatusCode();
            
            var serverDto = await response.Content.ReadFromJsonAsync<ServerComputerDto>();
            if (serverDto == null) return null;
            
            return new Models.ComputerDto
            {
                Id = serverDto.Id,
                Name = serverDto.Name,
                Specifications = serverDto.Specifications,
                Status = Enum.Parse<Models.ComputerStatus>(serverDto.Status, true),
                CurrentUserId = serverDto.CurrentUserId,
                PricePerHour = serverDto.PricePerHour
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка получения информации о компьютере: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    // Метод для создания заказа еды и напитков
    public async Task<OrderResponse?> CreateOrderAsync(OrderRequest orderRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/orders", orderRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    // Метод для получения всех заказов пользователя
    public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/users/{userId}/orders");
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

    // Метод для отмены заказа
    public async Task<bool> CancelOrderAsync(int orderId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{BaseUrl}/orders/{orderId}/cancel", null);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    // Метод для получения списка доступных компьютеров
    public async Task<List<Models.ComputerDto>> GetAvailableComputersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/computers/available");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var serverDtos = JsonSerializer.Deserialize<List<ServerComputerDto>>(content, _jsonOptions);
            
            // Маппинг из ServerComputerDto в Models.ComputerDto
            return serverDtos?.Select(dto => new Models.ComputerDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Specifications = dto.Specifications,
                Status = Enum.Parse<ClientComputerStatus>(dto.Status, true),
                CurrentUserId = dto.CurrentUserId,
                IpAddress = dto.IpAddress ?? string.Empty,
                MacAddress = dto.MacAddress ?? string.Empty,
                PricePerHour = dto.PricePerHour
            }).ToList() ?? new List<Models.ComputerDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка компьютеров: {ex.Message}");
            throw;
        }
    }

    // Метод для получения компьютера по ID
    public async Task<Models.ComputerDto> GetComputerByIdAsync(int computerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/computers/{computerId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var serverDto = JsonSerializer.Deserialize<ServerComputerDto>(content, _jsonOptions);
            
            // Маппинг из ServerComputerDto в Models.ComputerDto
            return serverDto != null ? new Models.ComputerDto
            {
                Id = serverDto.Id,
                Name = serverDto.Name,
                Specifications = serverDto.Specifications,
                Status = Enum.Parse<ClientComputerStatus>(serverDto.Status, true),
                CurrentUserId = serverDto.CurrentUserId,
                IpAddress = serverDto.IpAddress ?? string.Empty,
                MacAddress = serverDto.MacAddress ?? string.Empty,
                PricePerHour = serverDto.PricePerHour
            } : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении компьютера: {ex.Message}");
            throw;
        }
    }

    // Метод для получения списка продуктов питания
    public async Task<List<GameClubManager.Client.Models.FoodItem>> GetFoodItemsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/food");
            response.EnsureSuccessStatusCode();
            var sharedFoodItems = await response.Content.ReadFromJsonAsync<List<GameClubManager.Shared.Models.FoodItem>>();
            
            // Конвертируем из Shared.Models.FoodItem в Client.Models.FoodItem
            return sharedFoodItems?.Select(item => new GameClubManager.Client.Models.FoodItem
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description ?? string.Empty,
                Price = item.Price,
                Category = GameClubManager.Client.Models.FoodCategory.Food // Устанавливаем категорию по умолчанию
            }).ToList() ?? new List<GameClubManager.Client.Models.FoodItem>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при получении списка продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<GameClubManager.Client.Models.FoodItem>();
        }
    }
}

// Серверные DTO для десериализации ответов
public class ServerComputerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public int? CurrentUserId { get; set; }
    public string CurrentUserName { get; set; }
    public DateTime LastActivity { get; set; }
    public string Specifications { get; set; }
    public decimal PricePerHour { get; set; }
    public int? UserId { get; set; }
    public string IpAddress { get; set; }
    public string MacAddress { get; set; }
}

public class UserData
{
    public decimal Balance { get; set; }
    public TimeSpan RemainingTime { get; set; }
} 