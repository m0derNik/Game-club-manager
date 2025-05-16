using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameClubManager.Admin.Models
{
    public class Tariff : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _description;
        private decimal _price;
        private TimeSpan _duration;
        private bool _isPopular;

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
        
        public TimeSpan Duration 
        { 
            get => _duration; 
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsPopular 
        { 
            get => _isPopular; 
            set
            {
                _isPopular = value;
                OnPropertyChanged();
            }
        }
        
        // Вспомогательные свойства для UI
        public int Hours 
        { 
            get => Duration.Hours; 
            set 
            { 
                var newDuration = new TimeSpan(value, Minutes, 0);
                Duration = newDuration;
                OnPropertyChanged();
            } 
        }
        
        public int Minutes 
        { 
            get => Duration.Minutes; 
            set 
            { 
                var newDuration = new TimeSpan(Hours, value, 0);
                Duration = newDuration;
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