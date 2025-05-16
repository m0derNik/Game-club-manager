using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameClubManager.Server.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GamesController> _logger;

    public GamesController(ApplicationDbContext context, ILogger<GamesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetGames([FromQuery] bool availableOnly = false)
    {
        try
        {
            _logger.LogInformation("Запрос списка игр. ДоступныеТолько: {AvailableOnly}", availableOnly);
            
            IQueryable<Game> query = _context.Games;
            
            if (availableOnly)
            {
                query = query.Where(g => g.IsAvailable);
            }
            
            var games = await query.ToListAsync();
            
            _logger.LogInformation("Найдено игр: {Count}", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка игр");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<List<Game>>> GetAvailableGamesForUser()
    {
        try
        {
            // Логируем информацию о пользователе для отладки
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Пользователь аутентифицирован: {Username}", User.Identity.Name);
                _logger.LogInformation("Роли пользователя: {Roles}", string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));
            }
            else
            {
                _logger.LogWarning("Пользователь не аутентифицирован");
                return Unauthorized("Пользователь не аутентифицирован");
            }
            
            _logger.LogInformation("Запрос списка доступных игр для пользователя");
            
            var games = await _context.Games
                .Where(g => g.IsAvailable)
                .Select(g => new Game
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    ExecutablePath = g.ExecutablePath,
                    Genre = g.Genre,
                    IsAvailable = g.IsAvailable
                })
                .ToListAsync();
            
            _logger.LogInformation("Найдено доступных игр: {Count}", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка доступных игр для пользователя");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(int id)
    {
        try
        {
            _logger.LogInformation("Запрос игры с ID {GameId}", id);
            
            var game = await _context.Games.FindAsync(id);
            
            if (game == null)
            {
                _logger.LogWarning("Игра с ID {GameId} не найдена", id);
                return NotFound($"Игра с ID {id} не найдена");
            }
            
            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении игры с ID {GameId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpGet("genres")]
    public ActionResult<IEnumerable<string>> GetGenres()
    {
        try
        {
            _logger.LogInformation("Запрос списка жанров игр");
            
            var genres = Enum.GetNames(typeof(GameGenre)).ToList();
            return Ok(genres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка жанров игр");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpGet("popular")]
    public async Task<ActionResult<List<Game>>> GetPopularGames(int count = 5)
    {
        try
        {
            _logger.LogInformation("Запрос популярных игр. Количество: {Count}", count);
            
            var games = await _context.Games
                .Where(g => g.IsAvailable)
                .OrderByDescending(g => g.PopularityRating)
                .Take(count)
                .ToListAsync();
            
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении популярных игр");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<List<Game>>> GetGamesByGenre(string genre)
    {
        try
        {
            _logger.LogInformation("Запрос игр по жанру: {Genre}", genre);
            
            if (!Enum.TryParse<GameGenre>(genre, true, out var gameGenre))
            {
                _logger.LogWarning("Некорректный жанр: {Genre}", genre);
                return BadRequest($"Некорректный жанр: {genre}");
            }
            
            var games = await _context.Games
                .Where(g => g.Genre == gameGenre && g.IsAvailable)
                .ToListAsync();
            
            _logger.LogInformation("Найдено игр по жанру {Genre}: {Count}", genre, games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении игр по жанру {Genre}", genre);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Game>> CreateGame([FromBody] Game game)
    {
        // Проверяем на админский клиент
        if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
            adminClientValues.FirstOrDefault() == "true")
        {
            // Админский клиент, разрешено
        }
        else if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Недостаточно прав для создания игры");
        }
    
        try
        {
            if (game == null)
            {
                return BadRequest("Данные игры не могут быть пустыми");
            }
            
            // Сбрасываем ID для создания новой игры
            game.Id = 0;
            
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Создана новая игра с ID {GameId}: {GameName}", game.Id, game.Name);
            
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании игры");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGame(int id, [FromBody] Game game)
    {
        try
        {
            _logger.LogInformation("Запрос на обновление игры {GameId}. Пользователь аутентифицирован: {IsAuthenticated}, IsAdmin: {IsAdmin}", 
                id, User.Identity?.IsAuthenticated ?? false, User.IsInRole("Admin"));
            
            if (id != game.Id)
            {
                return BadRequest("ID в URL не совпадает с ID в теле запроса");
            }
            
            var existingGame = await _context.Games.FindAsync(id);
            if (existingGame == null)
            {
                _logger.LogWarning("Игра с ID {GameId} не найдена при попытке обновления", id);
                return NotFound($"Игра с ID {id} не найдена");
            }
            
            // Обновляем свойства
            _context.Entry(existingGame).CurrentValues.SetValues(game);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Обновлена игра с ID {GameId}: {GameName}", id, game.Name);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении игры с ID {GameId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        // Проверяем на админский клиент
        if (Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
            adminClientValues.FirstOrDefault() == "true")
        {
            // Админский клиент, разрешено
        }
        else if (!User.IsInRole("Admin"))
        {
            return Unauthorized("Недостаточно прав для удаления игры");
        }
    
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                _logger.LogWarning("Игра с ID {GameId} не найдена при попытке удаления", id);
                return NotFound($"Игра с ID {id} не найдена");
            }
            
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Удалена игра с ID {GameId}: {GameName}", id, game.Name);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении игры с ID {GameId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
    
    [HttpPut("{id}/toggle-availability")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<GameAvailabilityResponse>> ToggleGameAvailability(int id)
    {
        _logger.LogInformation("Запрос на изменение доступности игры {GameId}. Пользователь аутентифицирован: {IsAuthenticated}, IsAdmin: {IsAdmin}", 
            id, User.Identity?.IsAuthenticated ?? false, User.IsInRole("Admin"));
            
        try
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                _logger.LogWarning("Игра с ID {GameId} не найдена при попытке изменения доступности", id);
                return NotFound($"Игра с ID {id} не найдена");
            }

            // Инвертируем доступность
            game.IsAvailable = !game.IsAvailable;
            _context.Entry(game).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Изменен статус доступности игры {GameId} на {IsAvailable}", 
                id, game.IsAvailable);
            
            return Ok(new GameAvailabilityResponse
            {
                GameId = game.Id,
                IsAvailable = game.IsAvailable,
                UpdatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении доступности игры с ID {GameId}", id);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("public")]
    public async Task<ActionResult<List<Game>>> GetPublicGames()
    {
        try
        {
            _logger.LogInformation("Запрос публичного списка доступных игр");
            
            var games = await _context.Games
                .Where(g => g.IsAvailable)
                .Select(g => new Game
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    ExecutablePath = g.ExecutablePath,
                    Genre = g.Genre,
                    IsAvailable = g.IsAvailable
                })
                .ToListAsync();
            
            _logger.LogInformation("Найдено доступных игр: {Count}", games.Count);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении публичного списка доступных игр");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}

public class GameAvailabilityResponse
{
    public int GameId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime UpdatedAt { get; set; }
} 