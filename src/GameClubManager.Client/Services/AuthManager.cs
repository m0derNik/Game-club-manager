using GameClubManager.Client.Models.Auth;
using GameClubManager.Shared.Models;
using GameClubManager.Client.Dialogs;
using System.Windows;

namespace GameClubManager.Client.Services;

public class AuthManager
{
    private static AuthManager? _instance;
    private readonly ApiService _apiService;
    private GameClubManager.Shared.Models.AuthResponse? _currentUser;

    public static AuthManager Instance => _instance ??= new AuthManager();

    private AuthManager()
    {
        _apiService = new ApiService();
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var request = new GameClubManager.Shared.Models.RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password
            };

            var response = await _apiService.RegisterAsync(request);
            if (response != null)
            {
                _currentUser = response;
                _apiService.SetAuthToken(response.Token);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var request = new GameClubManager.Shared.Models.LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _apiService.LoginAsync(request);
            if (response != null)
            {
                _currentUser = response;
                _apiService.SetAuthToken(response.Token);
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

    public static bool VerifyPassword()
    {
        var passwordDialog = new PasswordDialog();
        if (passwordDialog.ShowDialog() == true)
        {
            return passwordDialog.Password == "admin"; // TODO: Заменить на реальную проверку
        }
        return false;
    }

    public bool IsAuthenticated => _currentUser != null;
    public GameClubManager.Shared.Models.UserDto? CurrentUser => _currentUser?.User;
    public string? Token => _currentUser?.Token;
} 