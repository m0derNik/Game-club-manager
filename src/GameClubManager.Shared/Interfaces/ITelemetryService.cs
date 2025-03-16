using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameClubManager.Shared.Models;

namespace GameClubManager.Shared.Interfaces
{
    public interface ITelemetryService
    {
        Task<ComputerTelemetry> GetLatestTelemetryAsync(int computerId);
        Task<IEnumerable<ComputerTelemetry>> GetTelemetryHistoryAsync(int computerId, DateTime from, DateTime to);
        Task<ComputerTelemetry> AddTelemetryDataAsync(ComputerTelemetry telemetry);
        Task<IEnumerable<SystemAlert>> GetActiveAlertsAsync(int computerId);
        Task<bool> AddAlertAsync(int computerId, SystemAlert alert);
        Task<bool> ResolveAlertAsync(int alertId);
        Task<IEnumerable<ProcessInfo>> GetRunningProcessesAsync(int computerId);
        Task<bool> TerminateProcessAsync(int computerId, string processName);
        Task<Dictionary<string, double>> GetResourceUsageStatisticsAsync(int computerId, TimeSpan period);
    }
} 