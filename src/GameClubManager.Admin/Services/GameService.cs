using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GameClubManager.Admin.Models;
using AdminGame = GameClubManager.Admin.Models.Game;
using SharedGame = GameClubManager.Shared.Models.Game;

namespace GameClubManager.Admin.Services
{
    public class GameService
    {
        private static GameService _instance;
        private readonly ApiService _apiService;
        private readonly ObservableCollection<AdminGame> _games = new ObservableCollection<AdminGame>();
        
        public static GameService Instance => _instance ??= new GameService();
        
        public ObservableCollection<AdminGame> Games => _games;
        
        private GameService()
        {
            _apiService = ApiService.Instance;
        }
        
        public async Task LoadGamesAsync()
        {
            var serverGames = await _apiService.GetGamesAsync();
            
            _games.Clear();
            
            if (serverGames != null)
            {
                foreach (var game in serverGames)
                {
                    _games.Add(ConvertToAdminGame(game));
                }
            }
        }
        
        public async Task<bool> AddGameAsync(AdminGame game)
        {
            var sharedGame = ConvertToSharedGame(game);
            var addedGame = await _apiService.AddGameAsync(sharedGame);
            
            if (addedGame != null)
            {
                var adminGame = ConvertToAdminGame(addedGame);
                _games.Add(adminGame);
                return true;
            }
            
            return false;
        }
        
        public async Task<bool> UpdateGameAsync(AdminGame game)
        {
            var sharedGame = ConvertToSharedGame(game);
            var success = await _apiService.UpdateGameAsync(sharedGame);
            
            if (success)
            {
                // Обновляем коллекцию
                for (int i = 0; i < _games.Count; i++)
                {
                    if (_games[i].Id == game.Id)
                    {
                        _games[i] = game;
                        break;
                    }
                }
            }
            
            return success;
        }
        
        public async Task<bool> DeleteGameAsync(int gameId)
        {
            var success = await _apiService.DeleteGameAsync(gameId);
            
            if (success)
            {
                // Удаляем из коллекции
                for (int i = 0; i < _games.Count; i++)
                {
                    if (_games[i].Id == gameId)
                    {
                        _games.RemoveAt(i);
                        break;
                    }
                }
            }
            
            return success;
        }
        
        public async Task<bool> ToggleGameAvailabilityAsync(int gameId)
        {
            var response = await _apiService.ToggleGameAvailabilityAsync(gameId);
            
            if (response != null)
            {
                // Обновляем флаг в коллекции
                for (int i = 0; i < _games.Count; i++)
                {
                    if (_games[i].Id == gameId)
                    {
                        _games[i].IsAvailable = response.IsAvailable;
                        break;
                    }
                }
                
                return true;
            }
            
            return false;
        }
        
        private AdminGame ConvertToAdminGame(SharedGame sharedGame)
        {
            return new AdminGame
            {
                Id = sharedGame.Id,
                Name = sharedGame.Name,
                Description = sharedGame.Description,
                Genre = sharedGame.Genre,
                ExecutablePath = sharedGame.ExecutablePath,
                IsAvailable = sharedGame.IsAvailable
            };
        }
        
        private SharedGame ConvertToSharedGame(AdminGame adminGame)
        {
            return new SharedGame
            {
                Id = adminGame.Id,
                Name = adminGame.Name,
                Description = adminGame.Description,
                Genre = adminGame.Genre,
                ExecutablePath = adminGame.ExecutablePath,
                IsAvailable = adminGame.IsAvailable
            };
        }
    }
} 