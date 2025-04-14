using System.Configuration;
using System.Data;
using System.Windows;
using System;
using System.Diagnostics;

namespace GameClubManager.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Добавляем DebugListener для отладки
        Trace.Listeners.Add(new TextWriterTraceListener("timer_log.txt"));
        Trace.AutoFlush = true;
        
        // Запись информации о запуске
        Trace.WriteLine($"Приложение запущено: {DateTime.Now}");
    }
}

