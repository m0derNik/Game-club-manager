using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameClubManager.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TariffsController : ControllerBase
    {
        private readonly ILogger<TariffsController> _logger;
        private readonly string _tariffsFilePath;
        private static readonly object _lock = new object();

        public TariffsController(ILogger<TariffsController> logger)
        {
            _logger = logger;
            
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var gameclubPath = Path.Combine(appDataPath, "GameClubManager");
            
            if (!Directory.Exists(gameclubPath))
            {
                Directory.CreateDirectory(gameclubPath);
            }
            
            _tariffsFilePath = Path.Combine(gameclubPath, "tariffs.json");
            
            // Если файл не существует, создаем его с базовыми тарифами
            if (!System.IO.File.Exists(_tariffsFilePath))
            {
                SaveTariffs(GetDefaultTariffs());
            }
        }

        // GET api/tariffs
        [HttpGet]
        public ActionResult<IEnumerable<Tariff>> Get()
        {
            try
            {
                var tariffs = LoadTariffs();
                return Ok(tariffs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении тарифов");
                return StatusCode(500, "Ошибка при получении тарифов");
            }
        }

        // GET api/tariffs/5
        [HttpGet("{id}")]
        public ActionResult<Tariff> Get(int id)
        {
            try
            {
                var tariffs = LoadTariffs();
                var tariff = tariffs.FirstOrDefault(t => t.Id == id);
                
                if (tariff == null)
                {
                    return NotFound($"Тариф с ID {id} не найден");
                }
                
                return Ok(tariff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при получении тарифа с ID {id}");
                return StatusCode(500, "Ошибка при получении тарифа");
            }
        }

        // POST api/tariffs
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<Tariff> Post([FromBody] Tariff tariff)
        {
            // Проверяем на админский клиент
            if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
                adminClientValues.FirstOrDefault() == "true")
            {
                // Админский клиент, разрешено
            }
            else if (!User.IsInRole("Admin"))
            {
                return Unauthorized("Недостаточно прав для создания тарифа");
            }

            try
            {
                if (tariff == null)
                {
                    return BadRequest("Тариф не может быть пустым");
                }
                
                var tariffs = LoadTariffs();
                
                // Назначаем новый ID
                tariff.Id = tariffs.Count > 0 ? tariffs.Max(t => t.Id) + 1 : 1;
                
                tariffs.Add(tariff);
                SaveTariffs(tariffs);
                
                return CreatedAtAction(nameof(Get), new { id = tariff.Id }, tariff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тарифа");
                return StatusCode(500, "Ошибка при создании тарифа");
            }
        }

        // PUT api/tariffs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Put(int id, [FromBody] Tariff tariff)
        {
            // Проверяем на админский клиент
            if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
                adminClientValues.FirstOrDefault() == "true")
            {
                // Админский клиент, разрешено
            }
            else if (!User.IsInRole("Admin"))
            {
                return Unauthorized("Недостаточно прав для обновления тарифа");
            }

            try
            {
                if (tariff == null)
                {
                    return BadRequest("Тариф не может быть пустым");
                }
                
                var tariffs = LoadTariffs();
                var existingTariff = tariffs.FirstOrDefault(t => t.Id == id);
                
                if (existingTariff == null)
                {
                    return NotFound($"Тариф с ID {id} не найден");
                }
                
                // Обновляем тариф
                var index = tariffs.IndexOf(existingTariff);
                tariff.Id = id; // Сохраняем исходный ID
                tariffs[index] = tariff;
                
                SaveTariffs(tariffs);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении тарифа с ID {id}");
                return StatusCode(500, "Ошибка при обновлении тарифа");
            }
        }

        // DELETE api/tariffs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            // Проверяем на админский клиент
            if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
                adminClientValues.FirstOrDefault() == "true")
            {
                // Админский клиент, разрешено
            }
            else if (!User.IsInRole("Admin"))
            {
                return Unauthorized("Недостаточно прав для удаления тарифа");
            }

            try
            {
                var tariffs = LoadTariffs();
                var tariff = tariffs.FirstOrDefault(t => t.Id == id);
                
                if (tariff == null)
                {
                    return NotFound($"Тариф с ID {id} не найден");
                }
                
                tariffs.Remove(tariff);
                SaveTariffs(tariffs);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении тарифа с ID {id}");
                return StatusCode(500, "Ошибка при удалении тарифа");
            }
        }
        
        private List<Tariff> LoadTariffs()
        {
            lock (_lock)
            {
                if (!System.IO.File.Exists(_tariffsFilePath))
                {
                    return GetDefaultTariffs();
                }
                
                var json = System.IO.File.ReadAllText(_tariffsFilePath);
                var tariffs = JsonSerializer.Deserialize<List<Tariff>>(json);
                
                return tariffs ?? GetDefaultTariffs();
            }
        }
        
        private void SaveTariffs(List<Tariff> tariffs)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(tariffs, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                System.IO.File.WriteAllText(_tariffsFilePath, json);
            }
        }
        
        private List<Tariff> GetDefaultTariffs()
        {
            return new List<Tariff>
            {
                new Tariff 
                { 
                    Id = 1, 
                    Name = "Базовый", 
                    Description = "Доступ к основным играм и сервисам",
                    Price = 100, 
                    Duration = TimeSpan.FromHours(1),
                    IsPopular = false
                },
                new Tariff 
                { 
                    Id = 2, 
                    Name = "Стандартный", 
                    Description = "Стандартный доступ к играм и сервисам",
                    Price = 250, 
                    Duration = TimeSpan.FromHours(3),
                    IsPopular = true
                },
                new Tariff 
                { 
                    Id = 3, 
                    Name = "Продвинутый", 
                    Description = "Расширенный доступ с приоритетным обслуживанием",
                    Price = 400, 
                    Duration = TimeSpan.FromHours(5),
                    IsPopular = false
                },
                new Tariff 
                { 
                    Id = 4, 
                    Name = "Ночной", 
                    Description = "Тариф для ночных игровых сессий",
                    Price = 500, 
                    Duration = TimeSpan.FromHours(8),
                    IsPopular = false
                },
                new Tariff 
                { 
                    Id = 5, 
                    Name = "Турнирный", 
                    Description = "Специальный тариф для игроков в турнирах",
                    Price = 1000, 
                    Duration = TimeSpan.FromHours(24),
                    IsPopular = false
                }
            };
        }
    }
} 