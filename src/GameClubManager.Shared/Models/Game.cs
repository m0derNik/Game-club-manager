using System;
using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models;

public class Game
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string ExecutablePath { get; set; } = string.Empty;
    
    public bool IsAvailable { get; set; } = true;
    
    public GameGenre Genre { get; set; }
    
    [StringLength(50)]
    public string Developer { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Publisher { get; set; } = string.Empty;
    
    public DateTime ReleaseDate { get; set; }
    
    public string ImageUrl { get; set; } = string.Empty;
    
    public string InstallationPath { get; set; } = string.Empty;
    
    public bool IsMultiplayer { get; set; }
    
    public int RequiredAgeRating { get; set; }
    
    [Range(1, 5)]
    public int PopularityRating { get; set; } = 3;
}

public enum GameGenre
{
    Action,
    Adventure,
    RPG,
    Simulation,
    Strategy,
    Sports,
    Racing,
    Puzzle,
    MOBA,
    Battle_Royale,
    FPS,
    Fighting,
    Horror,
    MMO,
    Platform,
    Other
} 