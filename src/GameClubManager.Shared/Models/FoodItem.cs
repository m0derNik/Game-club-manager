using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models;

public class FoodItem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsAvailable { get; set; } = true;
} 