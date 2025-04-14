using GameClubManager.Shared.Models;
using GameClubManager.Client.Dialogs;
using System.Windows;
using System.ComponentModel;
using System;
using System.Threading.Tasks;

namespace GameClubManager.Client.Services;

public class AuthManager : INotifyPropertyChanged
{
    private static AuthManager? _instance;
    private readonly ApiService _apiService;
    private readonly TimeService _timeService;
    private AuthResponse? _currentUser;

    public static AuthManager Instance => _instance ??= new AuthManager();
    public ApiService ApiService => _apiService;
    public event EventHandler? LoggedOut;
    public event EventHandler? LoggedIn;

    private AuthManager()
    {
        _apiService = ApiService.Instance;
        _timeService = TimeService.Instance;
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        try
        {
            var request = new RegisterRequest
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
                await _timeService.LoadUserDataAsync(response.User.Id);
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsAuthenticated));
                MessageBox.Show($"Успешная регистрация! Токен: {response.Token}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _apiService.LoginAsync(request);
            if (response != null)
            {
                _currentUser = response;
                _apiService.SetAuthToken(response.Token);
                
                // Вызываем событие перед загрузкой данных
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(IsAuthenticated));
                LoggedIn?.Invoke(this, EventArgs.Empty);
                
                // Загружаем данные асинхронно
                await _timeService.LoadUserDataAsync(response.User.Id);
                
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

    public async Task Logout()
    {
        try
        {
            if (_currentUser != null)
            {
                await _timeService.SaveUserData();
            }
            _currentUser = null;
            _apiService.SetAuthToken(null);
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(IsAuthenticated));
            LoggedOut?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    public UserDto? CurrentUser => _currentUser?.User;
    public string? Token => _currentUser?.Token;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 