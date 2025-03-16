using System.Collections.ObjectModel;
using System.Windows.Input;
using GameClubManager.Client.Models;

namespace GameClubManager.Client.ViewModels
{
    public class ProgramsViewModel : ViewModelBase
    {
        private ObservableCollection<Game> _games;
        public ObservableCollection<Game> Games
        {
            get => _games;
            set
            {
                _games = value;
                OnPropertyChanged(nameof(Games));
            }
        }

        public ICommand LaunchGameCommand { get; }

        public ProgramsViewModel()
        {
            Games = new ObservableCollection<Game>();
            LaunchGameCommand = new RelayCommand<Game>(LaunchGame);
            LoadGames();
        }

        private void LoadGames()
        {
            // Временные данные для демонстрации
            Games.Add(new Game { Name = "Counter-Strike 2", ExecutablePath = @"C:\Games\CS2\cs2.exe", IsEnabled = true });
            Games.Add(new Game { Name = "Dota 2", ExecutablePath = @"C:\Games\Dota2\dota2.exe", IsEnabled = true });
            Games.Add(new Game { Name = "Valorant", ExecutablePath = @"C:\Games\Valorant\valorant.exe", IsEnabled = true });
        }

        private void LaunchGame(Game game)
        {
            if (game != null && game.IsEnabled)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = game.ExecutablePath,
                    UseShellExecute = true
                });
            }
        }
    }
} 