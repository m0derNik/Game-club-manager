using System;

namespace GameClubManager.Client.Models
{
    public class ComputerStatusUpdateRequest
    {
        public ComputerStatus Status { get; set; }
        public int? UserId { get; set; }
    }
} 


