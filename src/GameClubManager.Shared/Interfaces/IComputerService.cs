using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;

namespace GameClubManager.Shared.Interfaces
{
    public interface IComputerService
    {
        Task<IEnumerable<Computer>> GetAllComputersAsync();
        Task<Computer> GetComputerByIdAsync(int id);
        Task<Computer> AddComputerAsync(Computer computer);
        Task<Computer> UpdateComputerAsync(Computer computer);
        Task<bool> DeleteComputerAsync(int id);
        Task<bool> UpdateComputerStatusAsync(int id, ComputerStatus status);
        Task<IEnumerable<InstalledGame>> GetInstalledGamesAsync(int computerId);
        Task<bool> ActivateGameLicenseAsync(int computerId, int gameId);
        Task<bool> DeactivateGameLicenseAsync(int computerId, int gameId);
        
        // Методы для удаленного управления
        Task<RemoteDesktopData> GetScreenshotAsync(int computerId);
        Task<bool> SendRemoteInputAsync(RemoteInput input);
        Task<bool> StartRemoteSessionAsync(int computerId);
        Task<bool> StopRemoteSessionAsync(int computerId);
    }
} 