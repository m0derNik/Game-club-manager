using GameClubManager.Server.Data;
using GameClubManager.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameClubManager.Server.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetAllOrders()
    {
        try
        {
            _logger.LogInformation("Запрос всех заказов");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.FoodItem)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.User.Username,
                    Items = o.Items.Select(i => new OrderItemResponse
                    {
                        Id = i.FoodItemId,
                        Name = i.FoodItem.Name,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList(),
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    DeliveryLocation = o.DeliveryLocation,
                    Comment = o.Comment
                })
                .ToListAsync();

            _logger.LogInformation("Найдено заказов: {Count}", orders.Count);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка всех заказов");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(OrderRequest request)
    {
        try
        {
            _logger.LogInformation("Создание нового заказа для пользователя {UserId}", request.UserId);

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь с ID {UserId} не найден", request.UserId);
                return NotFound($"Пользователь с ID {request.UserId} не найден");
            }

            if (user.Balance < request.TotalAmount)
            {
                _logger.LogWarning("Недостаточно средств у пользователя {UserId}. Требуется: {Amount}, Доступно: {Balance}", 
                    request.UserId, request.TotalAmount, user.Balance);
                return BadRequest("Недостаточно средств на балансе");
            }

            var order = new Order
            {
                UserId = request.UserId,
                TotalAmount = request.TotalAmount,
                DeliveryLocation = request.DeliveryLocation,
                Comment = request.Comment,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                Items = request.Items.Select(item => new OrderItem
                {
                    FoodItemId = item.Id,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            user.Balance -= request.TotalAmount;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Заказ успешно создан. ID: {OrderId}, Пользователь: {UserId}, Сумма: {Amount}", 
                order.Id, request.UserId, request.TotalAmount);

            // Получаем информацию о продуктах для ответа
            var foodItems = await _context.FoodItems
                .Where(fi => request.Items.Select(i => i.Id).Contains(fi.Id))
                .ToDictionaryAsync(fi => fi.Id, fi => fi);

            return Ok(new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = user.Username,
                Items = request.Items.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    Name = foodItems[item.Id].Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList(),
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                Status = order.Status,
                DeliveryLocation = order.DeliveryLocation,
                Comment = order.Comment
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании заказа для пользователя {UserId}", request.UserId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<OrderResponse>>> GetUserOrders(int userId)
    {
        try
        {
            _logger.LogInformation("Запрос заказов пользователя {UserId}", userId);

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.FoodItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserName = o.User.Username,
                    Items = o.Items.Select(i => new OrderItemResponse
                    {
                        Id = i.FoodItemId,
                        Name = i.FoodItem.Name,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList(),
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    DeliveryLocation = o.DeliveryLocation,
                    Comment = o.Comment
                })
                .ToListAsync();

            _logger.LogInformation("Найдено заказов: {Count}", orders.Count);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении заказов пользователя {UserId}", userId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        try
        {
            _logger.LogInformation("Запрос на отмену заказа {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Заказ с ID {OrderId} не найден", orderId);
                return NotFound($"Заказ с ID {orderId} не найден");
            }

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Невозможно отменить заказ {OrderId} со статусом {Status}", orderId, order.Status);
                return BadRequest("Невозможно отменить заказ в текущем статусе");
            }

            order.Status = OrderStatus.Canceled;
            order.User.Balance += order.TotalAmount;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Заказ {OrderId} успешно отменен", orderId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отмене заказа {OrderId}", orderId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("{orderId}/process")]
    public async Task<IActionResult> ProcessOrder(int orderId)
    {
        try
        {
            _logger.LogInformation("Запрос на обработку заказа {OrderId}", orderId);

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Заказ с ID {OrderId} не найден", orderId);
                return NotFound($"Заказ с ID {orderId} не найден");
            }

            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Невозможно обработать заказ {OrderId} со статусом {Status}", orderId, order.Status);
                return BadRequest($"Невозможно обработать заказ со статусом {order.Status}");
            }

            order.Status = OrderStatus.Processing;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Заказ {OrderId} переведен в статус обработки", orderId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке заказа {OrderId}", orderId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("{orderId}/complete")]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        try
        {
            _logger.LogInformation("Запрос на завершение заказа {OrderId}", orderId);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Заказ с ID {OrderId} не найден", orderId);
                return NotFound($"Заказ с ID {orderId} не найден");
            }

            if (order.Status != OrderStatus.Processing)
            {
                _logger.LogWarning("Невозможно завершить заказ {OrderId} со статусом {Status}", orderId, order.Status);
                return BadRequest($"Невозможно завершить заказ со статусом {order.Status}");
            }

            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Заказ {OrderId} завершен", orderId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении заказа {OrderId}", orderId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    [HttpPost("{orderId}/deliver")]
    public async Task<IActionResult> DeliverOrder(int orderId)
    {
        try
        {
            _logger.LogInformation("Запрос на доставку заказа {OrderId}", orderId);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Заказ с ID {OrderId} не найден", orderId);
                return NotFound($"Заказ с ID {orderId} не найден");
            }

            if (order.Status != OrderStatus.Completed)
            {
                _logger.LogWarning("Невозможно доставить заказ {OrderId} со статусом {Status}", orderId, order.Status);
                return BadRequest($"Невозможно доставить заказ со статусом {order.Status}");
            }

            order.Status = OrderStatus.Delivered;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Заказ {OrderId} доставлен", orderId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при доставке заказа {OrderId}", orderId);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
} 