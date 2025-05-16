using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GameClubManager.Shared.Models;

namespace GameClubManager.Admin.Models
{
    public class FoodItem : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private decimal _price;
        private string _imageUrl = string.Empty;
        private bool _isAvailable = true;
        private FoodCategory _category;

        public int Id 
        { 
            get => _id; 
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }
        
        public string Name 
        { 
            get => _name; 
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        
        public string Description 
        { 
            get => _description; 
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        
        public decimal Price 
        { 
            get => _price; 
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }
        
        public string ImageUrl 
        { 
            get => _imageUrl; 
            set
            {
                _imageUrl = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsAvailable 
        { 
            get => _isAvailable; 
            set
            {
                _isAvailable = value;
                OnPropertyChanged();
            }
        }
        
        public FoodCategory Category 
        { 
            get => _category; 
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 