using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models;

public class OrderItem
{
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; }
    
    [Required]
    public int FoodItemId { get; set; }
    public FoodItem FoodItem { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; }
} 