using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameClubManager.Server.Controllers
{
    [ApiController]
    [Route("api/computers")]
    public class ComputerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComputerController> _logger;

        public ComputerController(ApplicationDbContext context, ILogger<ComputerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Получение списка компьютеров
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComputerDto>>> GetComputers()
        {
            var computers = await _context.Computers
                .Select(c => new ComputerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status.ToString(),
                    CurrentUserId = c.CurrentUserId,
                    CurrentUserName = c.CurrentUserId.HasValue 
                        ? _context.Users.FirstOrDefault(u => u.Id == c.CurrentUserId).Username 
                        : null,
                    LastActivity = c.LastActivity,
                    Specifications = c.Specifications,
                    PricePerHour = c.PricePerHour
                })
                .ToListAsync();

            return Ok(computers);
        }

        // Регистрация компьютера (создание или обновление)
        [HttpPost("register")]
        public async Task<ActionResult<ComputerDto>> RegisterComputer(ComputerRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Регистрация компьютера: {ComputerName}", request.Name);

                // Проверяем, существует ли компьютер
                var computer = await _context.Computers
                    .FirstOrDefaultAsync(c => c.Name == request.Name);

                if (computer == null)
                {
                    // Создаем новый компьютер
                    computer = new Computer
                    {
                        Name = request.Name,
                        Specifications = request.Specifications,
                        Status = ComputerStatus.Available,
                        PricePerHour = request.PricePerHour,
                        LastActivity = DateTime.UtcNow
                    };

                    _context.Computers.Add(computer);
                    _logger.LogInformation("Создан новый компьютер: {ComputerName}", request.Name);
                }
                else
                {
                    // Обновляем существующий компьютер
                    computer.Specifications = request.Specifications;
                    computer.PricePerHour = request.PricePerHour;
                    computer.LastActivity = DateTime.UtcNow;
                    computer.Status = ComputerStatus.Available;
                    computer.CurrentUserId = null;

                    _logger.LogInformation("Обновлен существующий компьютер: {ComputerName}", request.Name);
                }

                await _context.SaveChangesAsync();

                // Возвращаем информацию о компьютере
                return Ok(new ComputerDto
                {
                    Id = computer.Id,
                    Name = computer.Name,
                    Status = computer.Status.ToString(),
                    CurrentUserId = computer.CurrentUserId,
                    CurrentUserName = null,
                    LastActivity = computer.LastActivity,
                    Specifications = computer.Specifications,
                    PricePerHour = computer.PricePerHour
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации компьютера: {ComputerName}", request.Name);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        // Обновление статуса компьютера
        [HttpPost("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, ComputerStatusUpdateRequest request)
        {
            try
            {
                var computer = await _context.Computers.FindAsync(id);
                if (computer == null)
                {
                    return NotFound($"Компьютер с ID {id} не найден");
                }

                computer.Status = request.Status;
                computer.LastActivity = DateTime.UtcNow;

                // Если статус "Используется", то обновляем текущего пользователя
                if (request.Status == ComputerStatus.InUse && request.UserId.HasValue)
                {
                    computer.CurrentUserId = request.UserId;
                    _logger.LogInformation("Компьютер {ComputerId} используется пользователем {UserId}", id, request.UserId);
                }
                // Если статус "Доступен", то очищаем текущего пользователя
                else if (request.Status == ComputerStatus.Available)
                {
                    computer.CurrentUserId = null;
                    _logger.LogInformation("Компьютер {ComputerId} теперь доступен", id);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса компьютера {ComputerId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        // Получение информации о компьютере по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ComputerDto>> GetComputer(int id)
        {
            var computer = await _context.Computers
                .Where(c => c.Id == id)
                .Select(c => new ComputerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status.ToString(),
                    CurrentUserId = c.CurrentUserId,
                    CurrentUserName = c.CurrentUserId.HasValue 
                        ? _context.Users.FirstOrDefault(u => u.Id == c.CurrentUserId).Username 
                        : null,
                    LastActivity = c.LastActivity,
                    Specifications = c.Specifications,
                    PricePerHour = c.PricePerHour
                })
                .FirstOrDefaultAsync();

            if (computer == null)
            {
                return NotFound($"Компьютер с ID {id} не найден");
            }

            return Ok(computer);
        }

        // Получение компьютера по имени
        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<ComputerDto>> GetComputerByName(string name)
        {
            var computer = await _context.Computers
                .Where(c => c.Name == name)
                .Select(c => new ComputerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status.ToString(),
                    CurrentUserId = c.CurrentUserId,
                    CurrentUserName = c.CurrentUserId.HasValue 
                        ? _context.Users.FirstOrDefault(u => u.Id == c.CurrentUserId).Username 
                        : null,
                    LastActivity = c.LastActivity,
                    Specifications = c.Specifications,
                    PricePerHour = c.PricePerHour
                })
                .FirstOrDefaultAsync();

            if (computer == null)
            {
                return NotFound($"Компьютер с именем {name} не найден");
            }

            return Ok(computer);
        }
    }

    // DTO для запросов и ответов
    public class ComputerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int? CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }
        public DateTime LastActivity { get; set; }
        public string Specifications { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class ComputerRegistrationRequest
    {
        public string Name { get; set; }
        public string Specifications { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class ComputerStatusUpdateRequest
    {
        public ComputerStatus Status { get; set; }
        public int? UserId { get; set; }
    }
} 