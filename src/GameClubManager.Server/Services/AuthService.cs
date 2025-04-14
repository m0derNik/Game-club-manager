using BCrypt.Net;
using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GameClubManager.Server.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<User?> GetUserByEmailAsync(string email);
}

public class AuthService(ApplicationDbContext context, IJwtService jwtService, ILogger<AuthService> logger) : IAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Начало регистрации пользователя: {Email}", request.Email);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            _logger.LogWarning("Попытка регистрации с существующим email: {Email}", request.Email);
            throw new Exception("Email уже зарегистрирован");
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            _logger.LogWarning("Попытка регистрации с существующим именем пользователя: {Username}", request.Username);
            throw new Exception("Имя пользователя уже занято");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = UserRole.User,
            Balance = 0
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Пользователь успешно зарегистрирован: {Email}", request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении пользователя: {Email}", request.Email);
            throw;
        }

        return user;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Попытка входа пользователя: {Email}", request.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            _logger.LogWarning("Пользователь не найден: {Email}", request.Email);
            throw new Exception("Пользователь не найден");
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Неверный пароль для пользователя: {Email}", request.Email);
            throw new Exception("Неверный пароль");
        }

        var token = _jwtService.GenerateToken(user);
        _logger.LogInformation("Пользователь успешно вошел: {Email}, ID: {UserId}", request.Email, user.Id);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Balance = user.Balance
            }
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
} 