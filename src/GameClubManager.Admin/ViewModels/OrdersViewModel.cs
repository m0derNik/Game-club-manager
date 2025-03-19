using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;

namespace GameClubManager.Admin.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orders;
        private string _selectedStatus;
        private DateTime? _selectedDate;
        private Order _selectedOrder;

        public OrdersViewModel()
        {
            // Временные данные для демонстрации UI
            Orders = new ObservableCollection<Order>
            {
                new Order { Id = 1, ComputerName = "PC-01", UserName = "Иван Иванов", TotalAmount = 150, Status = "Завершен", Date = DateTime.Now.AddHours(-2) },
                new Order { Id = 2, ComputerName = "PC-02", UserName = "Петр Петров", TotalAmount = 300, Status = "В процессе", Date = DateTime.Now.AddHours(-1) },
                new Order { Id = 3, ComputerName = "PC-03", UserName = "Сергей Сергеев", TotalAmount = 450, Status = "Завершен", Date = DateTime.Now.AddHours(-3) }
            };

            OrderStatuses = new[] { "Все", "В процессе", "Завершен", "Отменен" };
            SelectedStatus = "Все";

            ViewOrderCommand = new RelayCommand<Order>(ExecuteViewOrder);
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }

        public string[] OrderStatuses { get; }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                // Здесь будет логика фильтрации
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                // Здесь будет логика фильтрации
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }

        public ICommand ViewOrderCommand { get; }

        private void ExecuteViewOrder(Order order)
        {
            // Здесь будет логика просмотра заказа
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 