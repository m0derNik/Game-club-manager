using System;

namespace GameClubManager.Client.Models
{
    public class ComputerRegistrationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
    }
} 


