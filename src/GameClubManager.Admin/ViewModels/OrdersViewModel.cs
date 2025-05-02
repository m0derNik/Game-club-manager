using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Services;
using GameClubManager.Shared.Models;

namespace GameClubManager.Admin.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ObservableCollection<OrderResponse> _orders;
        private OrderResponse? _selectedOrder;
        private bool _isLoading;
        private bool _isProcessing;
        private bool _isEmptyVisible;
        private bool _isDataVisible;
        
        public ObservableCollection<OrderResponse> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged();
            }
        }
        
        public OrderResponse? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                UpdateVisibility();
            }
        }
        
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsEmptyVisible
        {
            get => _isEmptyVisible;
            set
            {
                _isEmptyVisible = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsDataVisible
        {
            get => _isDataVisible;
            set
            {
                _isDataVisible = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand LoadOrdersCommand { get; }
        public ICommand ProcessOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshOrdersCommand { get; }
        
        public OrdersViewModel()
        {
            _apiService = ApiService.Instance;
            _orders = new ObservableCollection<OrderResponse>();
            
            LoadOrdersCommand = new RelayCommand(_ => LoadOrdersAsync());
            ProcessOrderCommand = new RelayCommand<OrderResponse>(ProcessOrderAsync, CanProcessOrder);
            CompleteOrderCommand = new RelayCommand<OrderResponse>(CompleteOrderAsync, CanCompleteOrder);
            CancelOrderCommand = new RelayCommand<OrderResponse>(CancelOrderAsync, CanCancelOrder);
            ViewOrderDetailsCommand = new RelayCommand<OrderResponse>(ViewOrderDetails);
            RefreshOrdersCommand = new RelayCommand(_ => LoadOrdersAsync());
            
            LoadOrdersAsync();
        }
        
        private async void LoadOrdersAsync()
        {
            try
            {
                IsLoading = true;
                var orders = await _apiService.GetOrdersAsync();
                
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }
                
                UpdateVisibility();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void ProcessOrderAsync(OrderResponse? order)
        {
            if (order == null) return;
            
            try
            {
                IsProcessing = true;
                bool success = await _apiService.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing);
                
                if (success)
                {
                    order.Status = OrderStatus.Processing;
                    OnPropertyChanged(nameof(Orders));
                    MessageBox.Show($"Заказ #{order.Id} взят в обработку", 
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке заказа: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
        
        private async void CompleteOrderAsync(OrderResponse? order)
        {
            if (order == null) return;
            
            try
            {
                IsProcessing = true;
                bool success = await _apiService.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);
                
                if (success)
                {
                    Orders.Remove(order);
                    OnPropertyChanged(nameof(Orders));
                    UpdateVisibility();
                    MessageBox.Show($"Заказ #{order.Id} выполнен и удален из списка", 
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении заказа: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
        
        private async void CancelOrderAsync(OrderResponse? order)
        {
            if (order == null) return;
            
            var result = MessageBox.Show($"Вы действительно хотите отменить заказ #{order.Id}?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes)
                return;
            
            try
            {
                IsProcessing = true;
                bool success = await _apiService.UpdateOrderStatusAsync(order.Id, OrderStatus.Canceled);
                
                if (success)
                {
                    Orders.Remove(order);
                    OnPropertyChanged(nameof(Orders));
                    UpdateVisibility();
                    MessageBox.Show($"Заказ #{order.Id} отменен и удален из списка", 
                        "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
        
        private void ViewOrderDetails(OrderResponse? order)
        {
            if (order == null) return;
            SelectedOrder = order;
            
            // В реальном приложении здесь может быть код для открытия диалога с деталями
            MessageBox.Show($"Заказ #{order.Id}\n" +
                $"Пользователь: {order.UserName}\n" +
                $"Сумма: {order.TotalAmount:C}\n" +
                $"Дата: {order.OrderDate}\n" +
                $"Статус: {order.Status}\n" +
                $"Доставка: {order.DeliveryLocation}", 
                "Детали заказа", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private bool CanProcessOrder(OrderResponse? order)
        {
            return order != null && order.Status == OrderStatus.Pending && !IsProcessing;
        }
        
        private bool CanCompleteOrder(OrderResponse? order)
        {
            return order != null && order.Status == OrderStatus.Processing && !IsProcessing;
        }
        
        private bool CanCancelOrder(OrderResponse? order)
        {
            return order != null && 
                (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing) && 
                !IsProcessing;
        }
        
        private void UpdateVisibility()
        {
            IsEmptyVisible = !IsLoading && Orders.Count == 0;
            IsDataVisible = !IsLoading && Orders.Count > 0;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}