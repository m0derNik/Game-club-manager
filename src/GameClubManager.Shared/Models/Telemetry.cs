using System;

namespace GameClubManager.Shared.Models
{
    public class ComputerTelemetry
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double GpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double Temperature { get; set; }
        public List<ProcessInfo> RunningProcesses { get; set; } = new();
        public List<SystemAlert> Alerts { get; set; } = new();
    }

    public class ProcessInfo
    {
        public string Name { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public bool IsSuspicious { get; set; }
    }

    public class SystemAlert
    {
        public int Id { get; set; }
        public AlertType Type { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public AlertSeverity Severity { get; set; }
    }

    public enum AlertType
    {
        HighCpuUsage,
        HighMemoryUsage,
        HighTemperature,
        SuspiciousProcess,
        SystemError
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
} 