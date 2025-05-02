using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
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
            _logger.LogInformation("Запрос списка доступных продуктов питания");
            var foodItems = await _context.FoodItems
                .Where(f => f.IsAvailable)
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
} 