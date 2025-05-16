using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GameClubManager.Shared.Models;

namespace GameClubManager.Admin.Models
{
    public class Game : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private GameGenre _genre;
        private string _executablePath = string.Empty;
        private bool _isAvailable = true;

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
        
        public GameGenre Genre 
        { 
            get => _genre; 
            set
            {
                _genre = value;
                OnPropertyChanged();
            }
        }
        
        public string ExecutablePath 
        { 
            get => _executablePath; 
            set
            {
                _executablePath = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GameAvailabilityResponse
    {
        public int GameId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 