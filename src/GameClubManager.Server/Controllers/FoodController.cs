using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameClubManager.Server.Controllers;

[ApiController]
[Route("api/food")]
public class FoodController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FoodController> _logger;

    public FoodController(ApplicationDbContext context, ILogger<FoodController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<FoodItem>>> GetFoodItems()
    {
        try
        {
            _logger.LogInformation("Запрос списка продуктов питания");
            var foodItems = await _context.FoodItems
                .ToListAsync();
            
            _logger.LogInformation("Найдено продуктов питания: {Count}", foodItems.Count);
            return Ok(foodItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка продуктов питания");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FoodItem>> GetFoodItem(int id)
    {
        try
        {
            _logger.LogInformation("Запрос продукта питания с ID {FoodItemId}", id);
            var foodItem = await _context.FoodItems.FindAsync(id);
            
            if (foodItem == null)
            {
                _logger.LogWarning("Продукт питания с ID {FoodItemId} не найден", id);
                return NotFound($"Продукт питания с ID {id} не найден");
            }
            
            return Ok(foodItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении продукта питания с ID {FoodItemId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FoodItem>> CreateFoodItem([FromBody] FoodItem foodItem)
    {
        // Проверяем на админский клиент
        if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
            adminClientValues.FirstOrDefault() == "true")
        {
            // Админский клиент, разрешено
        }
        else if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Недостаточно прав для создания продукта");
        }
    
        try
        {
            if (foodItem == null)
            {
                return BadRequest("Данные продукта не могут быть пустыми");
            }
            
            // Сбрасываем ID для создания нового продукта
            foodItem.Id = 0;
            
            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Создан новый продукт питания с ID {FoodItemId}", foodItem.Id);
            
            return CreatedAtAction(nameof(GetFoodItem), new { id = foodItem.Id }, foodItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании продукта питания");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateFoodItem(int id, [FromBody] FoodItem foodItem)
    {
        // Проверяем на админский клиент
        if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
            adminClientValues.FirstOrDefault() == "true")
        {
            // Админский клиент, разрешено
        }
        else if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Недостаточно прав для обновления продукта");
        }
    
        try
        {
            if (id != foodItem.Id)
            {
                return BadRequest("ID в URL не совпадает с ID в теле запроса");
            }
            
            var existingFoodItem = await _context.FoodItems.FindAsync(id);
            if (existingFoodItem == null)
            {
                _logger.LogWarning("Продукт питания с ID {FoodItemId} не найден при попытке обновления", id);
                return NotFound($"Продукт питания с ID {id} не найден");
            }
            
            // Обновляем свойства
            existingFoodItem.Name = foodItem.Name;
            existingFoodItem.Description = foodItem.Description;
            existingFoodItem.Price = foodItem.Price;
            existingFoodItem.ImageUrl = foodItem.ImageUrl;
            existingFoodItem.IsAvailable = foodItem.IsAvailable;
            existingFoodItem.Category = foodItem.Category;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Обновлен продукт питания с ID {FoodItemId}", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении продукта питания с ID {FoodItemId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFoodItem(int id)
    {
        // Проверяем на админский клиент
        if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
            adminClientValues.FirstOrDefault() == "true")
        {
            // Админский клиент, разрешено
        }
        else if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Недостаточно прав для удаления продукта");
        }
    
        try
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                _logger.LogWarning("Продукт питания с ID {FoodItemId} не найден при попытке удаления", id);
                return NotFound($"Продукт питания с ID {id} не найден");
            }
            
            _context.FoodItems.Remove(foodItem);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Удален продукт питания с ID {FoodItemId}", id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении продукта питания с ID {FoodItemId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
} 