using System;

namespace GameClubManager.Shared.Models
{
    public class AdminNotification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? ComputerId { get; set; }
        public int? UserId { get; set; }
    }
} 