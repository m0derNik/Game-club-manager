using System;

namespace GameClubManager.Shared.Models
{
    /// <summary>
    /// Ответ на изменение доступности игры
    /// </summary>
    public class GameAvailabilityResponse
    {
        /// <summary>
        /// Идентификатор игры
        /// </summary>
        public int GameId { get; set; }
        
        /// <summary>
        /// Новое состояние доступности игры
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// Время изменения
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 