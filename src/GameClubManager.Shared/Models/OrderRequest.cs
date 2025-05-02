using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models;

public class OrderRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public string DeliveryLocation { get; set; }
    
    public string? Comment { get; set; }
    
    [Required]
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; }
} 