using System;
using System.Collections.Generic;

namespace GameClubManager.Shared.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public decimal Balance { get; set; }
        public List<Penalty> Penalties { get; set; } = new();
        public List<GamePreference> GamePreferences { get; set; } = new();
    }

    public enum UserRole
    {
        User,
        Admin,
        Manager
    }

    public class Penalty
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public bool IsPaid { get; set; }
    }

    public class GamePreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public int PlayTimeMinutes { get; set; }
        public DateTime LastPlayed { get; set; }
    }
} 