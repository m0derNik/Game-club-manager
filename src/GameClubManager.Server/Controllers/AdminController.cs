using GameClubManager.Server.Data;
using GameClubManager.Server.Services;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameClubManager.Server.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IJwtService _jwtService;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IJwtService jwtService)
        {
            _context = context;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Запрос списка всех пользователей");
                
                var users = await _context.Users.ToListAsync();
                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Balance = u.Balance
                }).ToList();
                
                _logger.LogInformation("Список пользователей успешно получен, найдено пользователей: {Count}", userDtos.Count);
                
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("users/{userId}/balance")]
        public async Task<IActionResult> AddUserBalance(int userId, [FromBody] AddBalanceRequest request)
        {
            try
            {
                _logger.LogInformation("Запрос на пополнение баланса пользователя ID: {UserId}, сумма: {Amount}", userId, request.Amount);
                
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                user.Balance += request.Amount;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Баланс пользователя ID: {UserId} успешно пополнен на {Amount}, текущий баланс: {Balance}", 
                    userId, request.Amount, user.Balance);
                
                return Ok(new { user.Balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при пополнении баланса пользователя ID: {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("users/{userId}/time")]
        public async Task<IActionResult> AddUserTime(int userId, [FromBody] AddTimeRequest request)
        {
            try
            {
                _logger.LogInformation("Запрос на добавление времени пользователю ID: {UserId}, минуты: {Minutes}", userId, request.Minutes);
                
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                var timeToAdd = TimeSpan.FromMinutes(request.Minutes);
                user.RemainingTime = user.RemainingTime.Add(timeToAdd);
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Время пользователя ID: {UserId} успешно увеличено на {Minutes} минут, текущее время: {RemainingTime}", 
                    userId, request.Minutes, user.RemainingTime);
                
                return Ok(new { FormattedRemainingTime = $"{user.RemainingTime.Hours}ч {user.RemainingTime.Minutes}м" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении времени пользователю ID: {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                _logger.LogInformation("Запрос на удаление пользователя ID: {UserId}", userId);
                
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                    return NotFound($"Пользователь с ID {userId} не найден");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Пользователь ID: {UserId} успешно удален", userId);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении пользователя ID: {UserId}", userId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("auth/login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> AdminLogin([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Попытка входа администратора: {Email}", request.Email);
                
                // В рамках упрощенного доступа, просто ищем пользователя с ролью Admin
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
                
                if (user == null)
                {
                    _logger.LogWarning("Администратор не найден в системе");
                    return Unauthorized("Администратор не настроен в системе");
                }

                // Для простоты не проверяем пароль
                // Генерируем токен для администратора
                var token = _jwtService.GenerateToken(user);
                
                _logger.LogInformation("Администратор успешно вошел: ID: {UserId}", user.Id);
                
                return Ok(new AuthResponse
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при входе администратора: {Email}", request.Email);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpPost("auth/auto-login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> AutoAdminLogin()
        {
            try
            {
                // Проверяем наличие заголовка X-Admin-Client
                if (!Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) ||
                    adminClientValues.FirstOrDefault() != "true")
                {
                    _logger.LogWarning("Попытка автоматического входа без специального заголовка");
                    return Unauthorized("Недостаточно прав для автоматического входа");
                }
                
                _logger.LogInformation("Автоматический вход администратора");
                
                // Ищем пользователя с ролью Admin
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
                
                if (user == null)
                {
                    _logger.LogWarning("Администратор не найден в системе");
                    return Unauthorized("Администратор не настроен в системе");
                }

                // Генерируем токен для администратора с расширенным сроком действия
                var token = _jwtService.GenerateToken(user, 24 * 30); // Токен на 30 дней
                
                _logger.LogInformation("Администратор успешно вошел автоматически: ID: {UserId}", user.Id);
                
                return Ok(new AuthResponse
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автоматическом входе администратора");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }

    // DTO-объекты для запросов и ответов
    public class AddBalanceRequest
    {
        public decimal Amount { get; set; }
    }

    public class AddTimeRequest
    {
        public int Minutes { get; set; }
    }
} 