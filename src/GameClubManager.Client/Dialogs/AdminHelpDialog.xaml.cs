using System.Windows;

namespace GameClubManager.Client.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для AdminHelpDialog.xaml
    /// </summary>
    public partial class AdminHelpDialog : Window
    {
        public string Reason { get; private set; }

        public AdminHelpDialog()
        {
            InitializeComponent();
            ReasonTextBox.Focus();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
            {
                System.Windows.MessageBox.Show("Пожалуйста, укажите причину вызова администратора", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Reason = ReasonTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 