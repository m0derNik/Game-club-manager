using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameClubManager.Shared.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public decimal Balance { get; set; }

        public TimeSpan RemainingTime { get; set; }

        public List<Penalty> Penalties { get; set; } = new();

        public virtual ICollection<GamePreference> GamePreferences { get; set; } = new List<GamePreference>();
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