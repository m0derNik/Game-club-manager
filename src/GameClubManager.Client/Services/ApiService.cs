using GameClubManager.Client.Models.Auth;
using GameClubManager.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GameClubManager.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:7001/api";

    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        _httpClient = new HttpClient(handler);
    }

    public async Task<GameClubManager.Shared.Models.AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameClubManager.Shared.Models.AuthResponse>();
        }
        catch (Exception ex)
        {
            // TODO: Добавить логирование
            throw;
        }
    }

    public async Task<GameClubManager.Shared.Models.AuthResponse?> LoginAsync(GameClubManager.Shared.Models.LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GameClubManager.Shared.Models.AuthResponse>();
        }
        catch (Exception ex)
        {
            // TODO: Добавить логирование
            throw;
        }
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
} 