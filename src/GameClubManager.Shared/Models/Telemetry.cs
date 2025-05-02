using System;
using System.Collections.Generic;

namespace GameClubManager.Shared.Models
{
    public class ComputerTelemetry
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double GpuUsage { get; set; }
        public int FreeSpaceGB { get; set; }
        public double Temperature { get; set; }
        public List<ProcessInfo> RunningProcesses { get; set; } = new();
        public List<SystemAlert> Alerts { get; set; } = new();
    }

    public class ProcessInfo
    {
        public int Id { get; set; }
        public int ComputerTelemetryId { get; set; }
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsageMB { get; set; }
        public bool IsGame { get; set; }
    }

    public class SystemAlert
    {
        public int Id { get; set; }
        public int ComputerTelemetryId { get; set; }
        public string Message { get; set; }
        public AlertSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsResolved { get; set; }
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

    // Добавления для удаленного рабочего стола
    public class RemoteDesktopData
    {
        public int ComputerId { get; set; }
        public byte[] ScreenData { get; set; }
        public bool IsCompressed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class RemoteInput
    {
        public int ComputerId { get; set; }
        public InputType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Button { get; set; } // 0 - левая, 1 - правая, 2 - средняя
        public int KeyCode { get; set; }
        public bool IsKeyDown { get; set; }
    }
    
    public enum InputType
    {
        MouseMove,
        MouseClick,
        MouseDown,
        MouseUp,
        MouseWheel,
        KeyPress,
        KeyDown,
        KeyUp
    }
} 