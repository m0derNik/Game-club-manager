using System;

namespace GameClubManager.Shared.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public int UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public List<GameSession> GameSessions { get; set; } = new();
    }

    public enum BookingStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled
    }

    public class GameSession
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    }
} 