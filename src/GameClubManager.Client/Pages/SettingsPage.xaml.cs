using System.Diagnostics;
using System.Windows.Controls;

namespace GameClubManager.Client.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }
        
        private void OpenMouseSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Открываем панель настроек мыши Windows
            Process.Start(new ProcessStartInfo
            {
                FileName = "control",
                Arguments = "main.cpl",
                UseShellExecute = true
            });
        }
    }
} 


