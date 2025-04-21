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

namespace GameClubManager.Client.Services;

public class ApiService
{
    private static ApiService? _instance;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:7001/api";

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
            var serverDto = await response.Content.ReadFromJsonAsync<ComputerDto>();
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
            
            var serverDtos = await response.Content.ReadFromJsonAsync<List<ComputerDto>>();
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
            
            var serverDto = await response.Content.ReadFromJsonAsync<ComputerDto>();
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
}

// Серверные DTO для десериализации ответов
public class ComputerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public int? CurrentUserId { get; set; }
    public string CurrentUserName { get; set; }
    public DateTime LastActivity { get; set; }
    public string Specifications { get; set; }
    public decimal PricePerHour { get; set; }
}

public class UserData
{
    public decimal Balance { get; set; }
    public TimeSpan RemainingTime { get; set; }
} 