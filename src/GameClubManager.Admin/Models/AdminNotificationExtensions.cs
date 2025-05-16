using System;

namespace GameClubManager.Admin.Models
{
    public static class AdminNotificationExtensions
    {
        public static Models.AdminNotification ToAdminModel(this GameClubManager.Shared.Models.AdminNotification source)
        {
            if (source == null) return null;
            
            return new Models.AdminNotification
            {
                Id = source.Id,
                Title = source.Title,
                Message = source.Message,
                Type = source.Type,
                CreatedAt = source.CreatedAt,
                IsRead = source.IsRead,
                ComputerId = source.ComputerId,
                UserId = source.UserId
            };
        }
        
        public static GameClubManager.Shared.Models.AdminNotification ToSharedModel(this Models.AdminNotification source)
        {
            if (source == null) return null;
            
            return new GameClubManager.Shared.Models.AdminNotification
            {
                Id = source.Id,
                Title = source.Title,
                Message = source.Message,
                Type = source.Type,
                CreatedAt = source.CreatedAt,
                IsRead = source.IsRead,
                ComputerId = source.ComputerId,
                UserId = source.UserId
            };
        }
    }
} 