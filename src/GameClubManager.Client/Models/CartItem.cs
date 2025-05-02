using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameClubManager.Client.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private FoodItem _foodItem;
        private int _quantity;
        
        public FoodItem FoodItem 
        { 
            get => _foodItem; 
            set
            {
                _foodItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
        
        public int Quantity 
        { 
            get => _quantity; 
            set
            {
                if (value < 1)
                    value = 1;
                
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
        
        public decimal TotalPrice => FoodItem.Price * Quantity;
        
        public CartItem(FoodItem foodItem, int quantity = 1)
        {
            _foodItem = foodItem;
            _quantity = quantity;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 