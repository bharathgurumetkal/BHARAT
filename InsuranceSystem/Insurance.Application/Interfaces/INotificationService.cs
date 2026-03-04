using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(Guid userId, string title, string message, string type);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
