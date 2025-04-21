using System;
using System.Collections.Generic;

namespace GameClubManager.Client.Models
{
    public class ComputerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public ComputerStatus Status { get; set; }
        public DateTime? BookedUntil { get; set; }
        public int? CurrentUserId { get; set; }
        public List<GameDto> InstalledGames { get; set; } = new List<GameDto>();
    }
    
    public enum ComputerStatus
    {
        Available,
        Occupied,
        OutOfService
    }
} 