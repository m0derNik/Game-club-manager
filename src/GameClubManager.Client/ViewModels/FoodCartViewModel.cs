using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;
using System.Windows;

namespace GameClubManager.Client.ViewModels
{
    public class FoodCartViewModel : INotifyPropertyChanged
    {
        private readonly FoodService _foodService;
        private readonly TimeService _timeService;
        private bool _isProcessing;
        
        public ObservableCollection<CartItem> CartItems => _foodService.Cart;
        
        public decimal CartTotal => _foodService.CartTotal;
        
        public decimal Balance => _timeService.Balance;
        
        public bool HasItems => CartItems.Count > 0;
        
        public bool IsProcessing 
        { 
            get => _isProcessing; 
            private set 
            { 
                _isProcessing = value; 
                OnPropertyChanged();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            } 
        }
        
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand PurchaseCommand { get; }
        public ICommand CloseCommand { get; }
        
        // События
        public event EventHandler CloseRequested;
        public event EventHandler<bool> PurchaseCompleted;
        
        public FoodCartViewModel()
        {
            _foodService = FoodService.Instance;
            _timeService = TimeService.Instance;
            
            // Подписываемся на изменения
            _timeService.PropertyChanged += OnTimeServicePropertyChanged;
            
            // Инициализируем команды
            IncreaseQuantityCommand = new RelayCommand<CartItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<CartItem>(DecreaseQuantity);
            RemoveFromCartCommand = new RelayCommand<int>(id => {
                if (id != 0)
                {
                    _foodService.RemoveFromCart(id);
                    OnPropertyChanged(nameof(CartItems));
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(HasItems));
                }
            });
            PurchaseCommand = new RelayCommand(_ => PurchaseItemsAsync(), _ => !IsProcessing);
            CloseCommand = new RelayCommand(_ => Close(_));
        }
        
        private void OnTimeServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeService.Balance))
            {
                OnPropertyChanged(nameof(Balance));
            }
        }
        
        private void IncreaseQuantity(CartItem? item)
        {
            if (item == null) return;
            
            item.Quantity++;
            OnPropertyChanged(nameof(CartTotal));
        }
        
        private void DecreaseQuantity(CartItem? item)
        {
            if (item == null || item.Quantity <= 1) return;
            
            item.Quantity--;
            OnPropertyChanged(nameof(CartTotal));
        }
        
        private async void PurchaseItemsAsync()
        {
            try
            {
                IsProcessing = true;
                
                if (_foodService.Cart.Count == 0)
                {
                    MessageBox.Show("Корзина пуста", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                bool success = await _foodService.PurchaseCartItems();
                
                if (success)
                {
                    OnPropertyChanged(nameof(CartItems));
                    OnPropertyChanged(nameof(CartTotal));
                    OnPropertyChanged(nameof(Balance));
                    OnPropertyChanged(nameof(HasItems));
                    
                    // Вызываем событие завершения покупки
                    PurchaseCompleted?.Invoke(this, true);
                    
                    // Закрываем диалог
                    CloseRequested?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }
        
        private void Close(object _)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 