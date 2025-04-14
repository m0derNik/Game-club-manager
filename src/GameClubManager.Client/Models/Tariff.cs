using System;

namespace GameClubManager.Client.Models
{
    public class Tariff
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsPopular { get; set; }
    }
} 