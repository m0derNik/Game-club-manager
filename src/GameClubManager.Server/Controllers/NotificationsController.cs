using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameClubManager.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ApplicationDbContext context, ILogger<NotificationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Получение списка уведомлений
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminNotification>>> GetNotifications()
        {
            return await _context.AdminNotifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Получение непрочитанных уведомлений
        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<AdminNotification>>> GetUnreadNotifications()
        {
            return await _context.AdminNotifications
                .Where(n => !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Создание уведомления о вызове администратора
        [HttpPost("call-admin")]
        public async Task<ActionResult<AdminNotification>> CallAdmin([FromBody] GameClubManager.Shared.Models.AdminCallRequest request)
        {
            try
            {
                _logger.LogInformation("Запрос на вызов администратора от компьютера: {ComputerName}", request.ComputerName);
                
                if (request == null)
                {
                    _logger.LogWarning("Получен пустой запрос на вызов администратора");
                    return BadRequest("Запрос не может быть пустым");
                }
                
                if (string.IsNullOrEmpty(request.ComputerName))
                {
                    _logger.LogWarning("Имя компьютера отсутствует в запросе");
                    return BadRequest("Имя компьютера не может быть пустым");
                }

                // Получаем информацию о компьютере
                var computer = await _context.Computers
                    .FirstOrDefaultAsync(c => c.Name == request.ComputerName);

                if (computer == null)
                {
                    _logger.LogWarning("Компьютер с именем {ComputerName} не найден", request.ComputerName);
                    return NotFound($"Компьютер с именем {request.ComputerName} не найден");
                }

                // Получаем информацию о пользователе
                string username = "Неизвестный пользователь";
                if (computer.CurrentUserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(computer.CurrentUserId.Value);
                    if (user != null)
                    {
                        username = user.Username;
                    }
                }

                // Создаем уведомление
                var notification = new AdminNotification
                {
                    Title = "Вызов администратора",
                    Message = $"Пользователь {username} на компьютере {computer.Name} запросил помощь администратора. Причина: {request.Reason}",
                    Type = "AdminCall",
                    ComputerId = computer.Id,
                    UserId = computer.CurrentUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                // Логирование данных перед сохранением
                _logger.LogInformation("Создаем уведомление: ID={Id}, Title={Title}, ComputerId={ComputerId}, UserId={UserId}", 
                    notification.Id, notification.Title, notification.ComputerId, notification.UserId);

                _context.AdminNotifications.Add(notification);
                
                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Создано уведомление о вызове администратора: ID {NotificationId}", notification.Id);
                    return Ok(notification);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Ошибка при сохранении уведомления в базу данных: {Message}", dbEx.InnerException?.Message);
                    
                    // Проверяем, связана ли ошибка с ограничениями NULL
                    if (dbEx.InnerException?.Message.Contains("Cannot insert the value NULL") == true)
                    {
                        // Если проблема с NULL-значениями, устанавливаем значения по умолчанию
                        if (notification.ComputerId == null)
                        {
                            _logger.LogWarning("ComputerId не может быть NULL, устанавливаем значение по умолчанию");
                            notification.ComputerId = 0; // Или другое значение по умолчанию
                        }
                        
                        if (notification.UserId == null)
                        {
                            _logger.LogWarning("UserId не может быть NULL, устанавливаем значение по умолчанию");
                            notification.UserId = 0; // Или другое значение по умолчанию
                        }
                        
                        // Пробуем сохранить еще раз
                        await _context.SaveChangesAsync();
                        return Ok(notification);
                    }
                    
                    return StatusCode(500, $"Ошибка сохранения в базу данных: {dbEx.InnerException?.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке запроса на вызов администратора");
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }

        // Отметить уведомление как прочитанное
        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var notification = await _context.AdminNotifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
        
        // Удалить уведомление (без строгой проверки аутентификации)
        [HttpPost("{id}/remove")]
        [AllowAnonymous]
        public async Task<ActionResult> RemoveNotification(int id)
        {
            try
            {
                _logger.LogInformation("Получен запрос на удаление уведомления: ID={Id}", id);
                
                // Проверяем наличие специального заголовка
                if (Request.Headers.TryGetValue("X-Admin-Action", out var adminAction))
                {
                    _logger.LogInformation("Обнаружен заголовок X-Admin-Action: {Value}", adminAction);
                }
                
                // Находим уведомление
                var notification = await _context.AdminNotifications.FindAsync(id);
                if (notification == null)
                {
                    _logger.LogWarning("Уведомление не найдено: ID={Id}", id);
                    return NotFound($"Уведомление с ID={id} не найдено");
                }
                
                // Удаляем уведомление
                _context.AdminNotifications.Remove(notification);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Уведомление успешно удалено: ID={Id}", id);
                return Ok(new { deleted = true, id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении уведомления: ID={Id}", id);
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
} 