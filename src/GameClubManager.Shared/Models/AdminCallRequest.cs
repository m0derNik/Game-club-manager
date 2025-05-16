using System;

namespace GameClubManager.Shared.Models
{
    /// <summary>
    /// Модель запроса на вызов администратора
    /// </summary>
    public class AdminCallRequest
    {
        /// <summary>
        /// Имя компьютера, с которого вызывают администратора
        /// </summary>
        public string ComputerName { get; set; }
        
        /// <summary>
        /// Причина вызова администратора
        /// </summary>
        public string Reason { get; set; }
    }
} 