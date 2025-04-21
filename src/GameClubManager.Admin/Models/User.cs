using System;
using System.Collections.Generic;

namespace GameClubManager.Admin.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal Balance { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public string Status { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<Penalty> Penalties { get; set; } = new List<Penalty>();
        
        public string FormattedRemainingTime
        {
            get
            {
                if (RemainingTime.TotalHours >= 1)
                {
                    return $"{RemainingTime.Hours}ч {RemainingTime.Minutes}м";
                }
                return $"{RemainingTime.Minutes}м";
            }
        }
    }
    
    public class Penalty
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public bool IsPaid { get; set; }
    }
} 