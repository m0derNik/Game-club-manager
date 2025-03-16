using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;

namespace GameClubManager.Shared.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId);
        Task<IEnumerable<Booking>> GetComputerBookingsAsync(int computerId);
        Task<bool> CancelBookingAsync(int id);
        Task<bool> CompleteBookingAsync(int id);
        Task<decimal> CalculateBookingPriceAsync(int computerId, DateTime startTime, DateTime endTime);
        Task<bool> IsComputerAvailableAsync(int computerId, DateTime startTime, DateTime endTime);
        Task<GameSession> StartGameSessionAsync(int bookingId, int gameId);
        Task<GameSession> EndGameSessionAsync(int sessionId);
    }
} 