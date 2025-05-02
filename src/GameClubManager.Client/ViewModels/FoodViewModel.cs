using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;
using GameClubManager.Client.Views;

namespace GameClubManager.Client.ViewModels
{
    public class FoodViewModel : INotifyPropertyChanged
    {
        private readonly FoodService _foodService;
        private readonly TimeService _timeService;
        private ObservableCollection<FoodItem> _foodItems;
        private ObservableCollection<CartItem> _cartItems;
        private bool _isCartVisible;
        
        public ObservableCollection<FoodItem> FoodItems => _foodItems;
        
        public ObservableCollection<CartItem> CartItems => _cartItems;
        
        public bool IsCartVisible
        {
            get => _isCartVisible;
            set
            {
                _isCartVisible = value;
                OnPropertyChanged();
            }
        }
        
        public decimal CartTotal => _foodService.CartTotal;
        
        public decimal Balance => _timeService.Balance;
        
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand ViewCartCommand { get; }
        public ICommand PurchaseCommand { get; }
        public ICommand HideCartCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        
        // События
        public event EventHandler? ShowCartRequested;
        
        public FoodViewModel()
        {
            _foodService = FoodService.Instance;
            _timeService = TimeService.Instance;
            
            // Инициализация коллекций
            _foodItems = new ObservableCollection<FoodItem>(_foodService.AvailableFoodItems);
            _cartItems = _foodService.Cart;
            
            // Подписка на изменения баланса
            _timeService.PropertyChanged += OnTimeServicePropertyChanged;
            
            // Инициализация команд
            AddToCartCommand = new RelayCommand<FoodItem>(AddToCart);
            ViewCartCommand = new RelayCommand(ShowCart);
            PurchaseCommand = new RelayCommand(PurchaseItems);
            HideCartCommand = new RelayCommand(HideCart);
            IncreaseQuantityCommand = new RelayCommand<CartItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<CartItem>(DecreaseQuantity);
            
            // Инициализируем команду удаления из корзины
            RemoveFromCartCommand = new RelayCommand<int>(id => {
                if (id != 0)
                {
                    _foodService.RemoveFromCart(id);
                    OnPropertyChanged(nameof(CartItems));
                    OnPropertyChanged(nameof(CartTotal));
                }
            });
        }
        
        private void OnTimeServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeService.Balance))
            {
                OnPropertyChanged(nameof(Balance));
            }
        }
        
        private void AddToCart(FoodItem? foodItem)
        {
            if (foodItem == null) return;
            
            _foodService.AddToCart(foodItem);
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(CartTotal));
        }
        
        private void ShowCart()
        {
            // Вызываем событие для отображения корзины в отдельном диалоговом окне
            ShowCartRequested?.Invoke(this, EventArgs.Empty);
        }
        
        private void HideCart()
        {
            IsCartVisible = false;
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
        
        private async void PurchaseItems()
        {
            bool success = await _foodService.PurchaseCartItems();
            if (success)
            {
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(CartTotal));
                OnPropertyChanged(nameof(Balance));
                IsCartVisible = false;
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 