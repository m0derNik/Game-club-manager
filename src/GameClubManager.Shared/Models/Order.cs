using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    public User User { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; }
    
    [Required]
    public string DeliveryLocation { get; set; }
    
    public string? Comment { get; set; }
    
    public List<OrderItem> Items { get; set; } = new();
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Delivered,
    Canceled
} 