using System.Configuration;
using System.Data;
using System.Windows;
using System;
using System.Diagnostics;
using GameClubManager.Client.Services;

namespace GameClubManager.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // ��������� DebugListener ��� �������
        Trace.Listeners.Add(new TextWriterTraceListener("timer_log.txt"));
        Trace.AutoFlush = true;
        
        // ������ ���������� � �������
        Trace.WriteLine($"���������� ��������: {DateTime.Now}");

        // �������������� ����������� �������
        TimeService.Instance.Initialize();
        
        // ��������� ������ ���������� ����������
        _ = RemoteControlService.Instance.RegisterForRemoteControlAsync();
        
        // ��������� �������������� ����������
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        System.Windows.MessageBox.Show($"��������� �������������� ������: {e.Exception.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}




