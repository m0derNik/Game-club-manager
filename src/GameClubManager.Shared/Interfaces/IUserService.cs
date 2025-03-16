using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;

namespace GameClubManager.Shared.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, string password);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> AddPenaltyAsync(int userId, Penalty penalty);
        Task<bool> ResolvePenaltyAsync(int penaltyId);
        Task<IEnumerable<Penalty>> GetUserPenaltiesAsync(int userId);
        Task<IEnumerable<GamePreference>> GetUserPreferencesAsync(int userId);
        Task<bool> UpdateGamePreferenceAsync(int userId, GamePreference preference);
        Task<IEnumerable<string>> GetGameRecommendationsAsync(int userId);
    }
} 