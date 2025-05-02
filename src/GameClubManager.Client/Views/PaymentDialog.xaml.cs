using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using GameClubManager.Client.Services;

namespace GameClubManager.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для PaymentDialog.xaml
    /// </summary>
    public partial class PaymentDialog : Window
    {
        private readonly TimeService _timeService;
        private decimal _amount = 100;
        
        public decimal Amount
        {
            get => _amount;
            set 
            {
                _amount = value;
                // Если требуется можно реализовать INotifyPropertyChanged
            }
        }
        
        public PaymentDialog()
        {
            InitializeComponent();
            DataContext = this;
            _timeService = TimeService.Instance;
            
            // Устанавливаем начальное значение
            AmountTextBox.Text = "100";
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем сумму
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную сумму", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Имитация процесса оплаты
            var paymentMethod = CreditCardRadio.IsChecked == true ? "Банковская карта" : 
                QiwiRadio.IsChecked == true ? "QIWI Кошелек" : "Яндекс Деньги";
            
            // Эмуляция задержки для реалистичности
            Mouse.OverrideCursor = Cursors.Wait;
            
            try
            {
                // Имитируем процесс оплаты
                System.Threading.Thread.Sleep(2000);
                
                // Добавляем средства на баланс
                _timeService.AddBalance(amount);
                
                MessageBox.Show($"Баланс успешно пополнен на {amount:C}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при пополнении баланса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
} 