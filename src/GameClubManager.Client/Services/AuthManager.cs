using GameClubManager.Shared.Models;
using GameClubManager.Client.Dialogs;
using System.Windows;
using System.ComponentModel;
using System;
using System.Threading.Tasks;
using GameClubManager.Client.Models;

namespace GameClubManager.Client.Services;

// Результат аутентификации
public class AuthResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class AuthManager : INotifyPropertyChanged
{
    private static AuthManager? _instance;
    private readonly ApiService _apiService;
    private readonly TimeService _timeService;
    private AuthResponse? _currentUser;
    private ComputerDto? _currentComputer;

    public static AuthManager Instance => _instance ??= new AuthManager();
    public ApiService ApiService => _apiService;
    public event EventHandler? LoggedOut;
    public event EventHandler? LoggedIn;

    private AuthManager()
    {
        _apiService = ApiService.Instance;
        _timeService = TimeService.Instance;
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password)
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
                return new AuthResult { Success = true };
            }
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = "Не удалось создать аккаунт. Пожалуйста, попробуйте позже." 
            };
        }
        catch (Exception ex)
        {
            // Обработка известных ошибок и возврат понятных сообщений
            if (ex.Message.Contains("Email уже зарегистрирован"))
            {
                return new AuthResult { Success = false, ErrorMessage = "Пользователь с таким email уже существует" };
            }
            else if (ex.Message.Contains("Имя пользователя уже занято"))
            {
                return new AuthResult { Success = false, ErrorMessage = "Это имя пользователя уже занято" };
            }
            else if (ex.Message.Contains("Bad Request") || ex.Message.Contains("400"))
            {
                return new AuthResult { Success = false, ErrorMessage = "Неверные данные. Проверьте введенную информацию" };
            }
            else if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                return new AuthResult { Success = false, ErrorMessage = "Ошибка авторизации" };
            }
            else if (ex.Message.Contains("Forbidden") || ex.Message.Contains("403"))
            {
                return new AuthResult { Success = false, ErrorMessage = "Доступ запрещен" };
            }
            else if (ex.Message.Contains("Not Found") || ex.Message.Contains("404"))
            {
                return new AuthResult { Success = false, ErrorMessage = "Сервер не доступен. Попробуйте позже" };
            }
            else
            {
                return new AuthResult { Success = false, ErrorMessage = "Произошла ошибка при регистрации. Попробуйте позже" };
            }
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
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
                
                return new AuthResult { Success = true };
            }
            return new AuthResult 
            { 
                Success = false, 
                ErrorMessage = "Неверно введенные данные" 
            };
        }
        catch (Exception ex)
        {
            // По умолчанию для всех ошибок входа используем сообщение о неверных данных
            string errorMessage = "Неверно введенные данные";
            
            // Проверяем конкретные случаи, когда нужно показать другое сообщение
            if (ex.Message.Contains("Not Found") || ex.Message.Contains("404"))
            {
                errorMessage = "Сервер не доступен. Попробуйте позже";
            }
            else if (ex.Message.Contains("timeout") || ex.Message.Contains("timed out"))
            {
                errorMessage = "Превышено время ожидания ответа от сервера";
            }
            
            return new AuthResult { Success = false, ErrorMessage = errorMessage };
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
            System.Windows.MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    public int CurrentUserId => _currentUser?.User?.Id ?? 0;
    
    // Свойство для доступа к текущему компьютеру
    public ComputerDto? CurrentComputer 
    { 
        get => _currentComputer;
        set
        {
            _currentComputer = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 


