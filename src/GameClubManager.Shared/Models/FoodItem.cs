using System;
using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models;

public class FoodItem
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsAvailable { get; set; } = true;
    
    public string ImageUrl { get; set; } = string.Empty;
    
    public FoodCategory Category { get; set; }
}

public enum FoodCategory
{
    Food,
    Drink,
    Snack
} 