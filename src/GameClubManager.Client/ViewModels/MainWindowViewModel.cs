using System.ComponentModel;
using System.Runtime.CompilerServices;
using GameClubManager.Client.Services;

namespace GameClubManager.Client.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly TimeService _timeService;
        private string _title = "Game Club Manager";

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public decimal Balance => _timeService.Balance;
        public string RemainingTime => _timeService.RemainingTime;

        public MainWindowViewModel()
        {
            _timeService = TimeService.Instance;
            
            // Подписываемся на изменения времени и баланса
            _timeService.PropertyChanged += TimeService_PropertyChanged;
        }

        private void TimeService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeService.RemainingTime))
                OnPropertyChanged(nameof(RemainingTime));
            else if (e.PropertyName == nameof(TimeService.Balance))
                OnPropertyChanged(nameof(Balance));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 