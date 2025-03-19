using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;

namespace GameClubManager.Admin.ViewModels
{
    public class ComputersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Computer> _computers;
        private string _searchText;
        private Computer _selectedComputer;

        public ComputersViewModel()
        {
            // Временные данные для демонстрации UI
            Computers = new ObservableCollection<Computer>
            {
                new Computer { Id = 1, Name = "PC-01", Status = "Свободен", CurrentUser = "-", TimeRemaining = "-" },
                new Computer { Id = 2, Name = "PC-02", Status = "Занят", CurrentUser = "Иван Иванов", TimeRemaining = "1:30" },
                new Computer { Id = 3, Name = "PC-03", Status = "Свободен", CurrentUser = "-", TimeRemaining = "-" },
                new Computer { Id = 4, Name = "PC-04", Status = "Занят", CurrentUser = "Петр Петров", TimeRemaining = "0:45" },
                new Computer { Id = 5, Name = "PC-05", Status = "Свободен", CurrentUser = "-", TimeRemaining = "-" }
            };

            RestartCommand = new RelayCommand<Computer>(ExecuteRestart);
            ShutdownCommand = new RelayCommand<Computer>(ExecuteShutdown);
        }

        public ObservableCollection<Computer> Computers
        {
            get => _computers;
            set
            {
                _computers = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Здесь будет логика фильтрации
            }
        }

        public Computer SelectedComputer
        {
            get => _selectedComputer;
            set
            {
                _selectedComputer = value;
                OnPropertyChanged();
            }
        }

        public ICommand RestartCommand { get; }
        public ICommand ShutdownCommand { get; }

        private void ExecuteRestart(Computer computer)
        {
            // Здесь будет логика перезапуска
        }

        private void ExecuteShutdown(Computer computer)
        {
            // Здесь будет логика выключения
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 