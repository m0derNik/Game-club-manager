using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;
using System.Windows;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

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
}

public class UserData
{
    public decimal Balance { get; set; }
    public TimeSpan RemainingTime { get; set; }
} 