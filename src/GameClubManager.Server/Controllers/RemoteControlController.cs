using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameClubManager.Server.Controllers
{
    [ApiController]
    [Route("api/remotecontrol")]
    public class RemoteControlController : ControllerBase
    {
        private readonly ILogger<RemoteControlController> _logger;
        
        // Хранилища состояний для каждого компьютера
        private static readonly ConcurrentDictionary<int, RemoteDesktopData> _screenshots = new();
        private static readonly ConcurrentDictionary<int, List<RemoteInput>> _pendingCommands = new();
        private static readonly ConcurrentDictionary<int, bool> _activeSessions = new();
        
        public RemoteControlController(ILogger<RemoteControlController> logger)
        {
            _logger = logger;
        }
        
        // Метод для получения скриншота конкретного компьютера
        [HttpGet("{computerId}/screenshot")]
        public ActionResult<RemoteDesktopData> GetScreenshot(int computerId)
        {
            try
            {
                if (_screenshots.TryGetValue(computerId, out var screenshot))
                {
                    return Ok(screenshot);
                }
                
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении скриншота для компьютера {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для отправки скриншота от клиента
        [HttpPost("screenshot")]
        public ActionResult SaveScreenshot([FromBody] RemoteDesktopData screenshotData)
        {
            try
            {
                if (screenshotData == null || screenshotData.ScreenData == null)
                {
                    return BadRequest("Некорректные данные скриншота");
                }
                
                // Если сессия неактивна, игнорируем скриншоты
                if (!_activeSessions.TryGetValue(screenshotData.ComputerId, out var isActive) || !isActive)
                {
                    return Ok();
                }
                
                // Сохраняем скриншот
                _screenshots[screenshotData.ComputerId] = screenshotData;
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении скриншота");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для отправки команд на клиент
        [HttpPost("{computerId}/command")]
        public ActionResult SendCommand(int computerId, [FromBody] RemoteInput command)
        {
            try
            {
                if (command == null)
                {
                    return BadRequest("Некорректные данные команды");
                }
                
                if (!_activeSessions.TryGetValue(computerId, out var isActive) || !isActive)
                {
                    return BadRequest("Сессия управления не активна");
                }
                
                // Инициализируем список команд, если его нет
                if (!_pendingCommands.TryGetValue(computerId, out var commands))
                {
                    commands = new List<RemoteInput>();
                    _pendingCommands[computerId] = commands;
                }
                
                // Добавляем команду
                commands.Add(command);
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке команды на компьютер {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для получения клиентом ожидающих команд
        [HttpGet("{computerId}/commands")]
        public ActionResult<List<RemoteInput>> GetPendingCommands(int computerId)
        {
            try
            {
                if (_pendingCommands.TryGetValue(computerId, out var commands))
                {
                    // Очищаем список команд
                    _pendingCommands[computerId] = new List<RemoteInput>();
                    return Ok(commands);
                }
                
                return Ok(new List<RemoteInput>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ожидающих команд для компьютера {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для начала сессии удаленного управления
        [HttpPost("{computerId}/start")]
        public ActionResult StartRemoteSession(int computerId)
        {
            try
            {
                _activeSessions[computerId] = true;
                _logger.LogInformation("Начата сессия удаленного управления для компьютера {ComputerId}", computerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале сессии удаленного управления для компьютера {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для остановки сессии удаленного управления
        [HttpPost("{computerId}/stop")]
        public ActionResult StopRemoteSession(int computerId)
        {
            try
            {
                _activeSessions[computerId] = false;
                _logger.LogInformation("Остановлена сессия удаленного управления для компьютера {ComputerId}", computerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при остановке сессии удаленного управления для компьютера {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для получения статуса сессии
        [HttpGet("{computerId}/status")]
        public ActionResult<bool> GetSessionStatus(int computerId)
        {
            try
            {
                if (_activeSessions.TryGetValue(computerId, out var isActive))
                {
                    return Ok(isActive);
                }
                
                return Ok(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статуса сессии для компьютера {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
} 