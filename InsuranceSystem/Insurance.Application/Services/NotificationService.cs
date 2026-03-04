using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task CreateAsync(Guid userId, string title, string message, string type)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _notificationRepository.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }
    }
}
