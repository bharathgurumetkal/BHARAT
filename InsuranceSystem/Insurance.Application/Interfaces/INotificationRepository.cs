using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task SaveChangesAsync();
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
