using System;

namespace GameClubManager.Admin.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public bool IsRead { get; set; }
    }
} 