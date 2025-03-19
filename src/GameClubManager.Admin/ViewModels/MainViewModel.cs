using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameClubManager.Admin.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _currentPageTitle;

        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set
            {
                _currentPageTitle = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            CurrentPageTitle = "Компьютеры";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 