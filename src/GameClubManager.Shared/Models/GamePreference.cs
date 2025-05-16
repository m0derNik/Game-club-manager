using System;

namespace GameClubManager.Shared.Models;

public class GamePreference
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int GameId { get; set; }
    
    public DateTime LastPlayed { get; set; }
    
    public TimeSpan TotalPlaytime { get; set; }
    
    public bool IsFavorite { get; set; }
} 