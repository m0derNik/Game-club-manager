using System;

namespace GameClubManager.Admin.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string CurrentUser { get; set; }
        public string TimeRemaining { get; set; }
    }
} 