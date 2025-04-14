using GameClubManager.Server.Data;
using GameClubManager.Server.Services;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameClubManager.Server.Controllers;

[ApiController]
[Route("api/users")]
public class UserDataController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserDataController> _logger;

    public UserDataController(ApplicationDbContext context, ILogger<UserDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{userId}/data")]
    public async Task<ActionResult<UserData>> GetUserData(int userId)
    {
        try
        {
            _logger.LogInformation("Запрос данных пользователя с ID: {UserId}", userId);
            
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                return NotFound($"Пользователь с ID {userId} не найден");
            }

            _logger.LogInformation("Найден пользователь: {Username} (ID: {UserId})", user.Username, userId);

            var userData = new UserData
            {
                Balance = user.Balance,
                RemainingTime = user.RemainingTime
            };

            _logger.LogInformation("Данные пользователя {UserId} успешно загружены. Баланс: {Balance}, Оставшееся время: {RemainingTime}", 
                userId, userData.Balance, userData.RemainingTime);
            
            return Ok(userData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке данных пользователя {UserId}", userId);
            return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
        }
    }

    [HttpPut("{userId}/data")]
    public async Task<IActionResult> UpdateUserData(int userId, UserData userData)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден", userId);
                return NotFound();
            }

            user.Balance = userData.Balance;
            user.RemainingTime = userData.RemainingTime;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Данные пользователя {UserId} успешно обновлены", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении данных пользователя {UserId}", userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
} 