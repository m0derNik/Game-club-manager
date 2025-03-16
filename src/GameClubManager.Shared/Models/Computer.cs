using System;

namespace GameClubManager.Shared.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specifications { get; set; }
        public bool IsAvailable { get; set; }
        public decimal PricePerHour { get; set; }
        public ComputerStatus Status { get; set; }
        public List<InstalledGame> InstalledGames { get; set; } = new();
    }

    public enum ComputerStatus
    {
        Available,
        InUse,
        Maintenance,
        OutOfOrder
    }

    public class InstalledGame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; } // Steam, Epic, etc.
        public bool IsLicenseActive { get; set; }
    }
} 