using System;

namespace GameClubManager.Admin.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
} 