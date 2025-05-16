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
        
        // Публичный метод для получения скриншота без авторизации
        [HttpGet("{computerId}/public-screenshot")]
        public ActionResult<RemoteDesktopData> GetPublicScreenshot(int computerId)
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
                _logger.LogError(ex, "Ошибка при получении публичного скриншота для компьютера {ComputerId}", computerId);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для отправки скриншота от клиента
        [HttpPost("screenshot")]
        public ActionResult UploadScreenshot([FromBody] RemoteDesktopData screenshot)
        {
            try
            {
                if (screenshot == null)
                {
                    return BadRequest("Некорректные данные скриншота");
                }
                
                // Обновляем скриншот для нужного компьютера
                _screenshots[screenshot.ComputerId] = screenshot;
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении скриншота от компьютера {ComputerId}", 
                    screenshot?.ComputerId ?? 0);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Публичный метод для отправки скриншота от клиента
        [HttpPost("public-screenshot")]
        public ActionResult UploadPublicScreenshot([FromBody] RemoteDesktopData screenshot)
        {
            try
            {
                if (screenshot == null)
                {
                    return BadRequest("Некорректные данные скриншота");
                }
                
                // Обновляем скриншот для нужного компьютера
                _screenshots[screenshot.ComputerId] = screenshot;
                
                // Уменьшаем частоту логирования для часто вызываемого метода
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении публичного скриншота от компьютера {ComputerId}", 
                    screenshot?.ComputerId ?? 0);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
        
        // Метод для отправки команды удаленного управления
        [HttpPost("{computerId}/command")]
        public ActionResult SendRemoteCommand(int computerId, [FromBody] RemoteInput command)
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
        
        // Публичный метод для отправки команды удаленного управления
        [HttpPost("{computerId}/public-command")]
        public ActionResult SendPublicRemoteCommand(int computerId, [FromBody] RemoteInput command)
        {
            try
            {
                _logger.LogInformation("Получена публичная команда для компьютера {ComputerId}", computerId);
                
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
                _logger.LogError(ex, "Ошибка при отправке публичной команды на компьютер {ComputerId}", computerId);
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
        
        // Публичный метод для получения команд, который не требует авторизации
        [HttpGet("{computerId}/public-commands")]
        public ActionResult<List<RemoteInput>> GetPublicPendingCommands(int computerId)
        {
            try
            {
                if (_pendingCommands.TryGetValue(computerId, out var commands))
                {
                    // Очищаем список команд
                    _pendingCommands[computerId] = new List<RemoteInput>();
                    
                    // Логируем только если есть команды для выполнения (уменьшаем спам)
                    if (commands.Count > 0)
                    {
                        _logger.LogInformation("Возвращено {Count} команд для компьютера {ComputerId}", commands.Count, computerId);
                    }
                    
                    return Ok(commands);
                }
                
                return Ok(new List<RemoteInput>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении публичных команд для компьютера {ComputerId}", computerId);
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
        
        // Публичный метод для начала сессии удаленного управления
        [HttpPost("{computerId}/public-start")]
        public ActionResult StartPublicRemoteSession(int computerId)
        {
            try
            {
                _activeSessions[computerId] = true;
                _logger.LogInformation("Начата публичная сессия удаленного управления для компьютера {ComputerId}", computerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при начале публичной сессии удаленного управления для компьютера {ComputerId}", computerId);
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
        
        // Публичный метод для остановки сессии удаленного управления
        [HttpPost("{computerId}/public-stop")]
        public ActionResult StopPublicRemoteSession(int computerId)
        {
            try
            {
                _activeSessions[computerId] = false;
                _logger.LogInformation("Остановлена публичная сессия удаленного управления для компьютера {ComputerId}", computerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при остановке публичной сессии удаленного управления для компьютера {ComputerId}", computerId);
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