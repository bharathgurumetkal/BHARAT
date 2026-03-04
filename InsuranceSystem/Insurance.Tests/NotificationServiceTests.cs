using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> _notificationRepositoryMock;
        private NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _notificationService = new NotificationService(_notificationRepositoryMock.Object);
        }

        // ─── CreateAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidData_SavesNotificationWithCorrectFields()
        {
            var userId = Guid.NewGuid();

            await _notificationService.CreateAsync(userId, "Alert", "Something happened", "Warning");

            _notificationRepositoryMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
                n.UserId  == userId       &&
                n.Title   == "Alert"      &&
                n.Message == "Something happened" &&
                n.Type    == "Warning"    &&
                !n.IsRead &&
                n.Id      != Guid.Empty
            )), Times.Once);
            _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_SetsIsReadToFalse()
        {
            await _notificationService.CreateAsync(Guid.NewGuid(), "T", "M", "Info");

            _notificationRepositoryMock.Verify(r => r.AddAsync(It.Is<Notification>(n => n.IsRead == false)), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_GeneratesUniqueId()
        {
            Guid capturedId1 = Guid.Empty;
            Guid capturedId2 = Guid.Empty;

            _notificationRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .Callback<Notification>(n =>
                {
                    if (capturedId1 == Guid.Empty) capturedId1 = n.Id;
                    else capturedId2 = n.Id;
                });

            await _notificationService.CreateAsync(Guid.NewGuid(), "A", "B", "Info");
            await _notificationService.CreateAsync(Guid.NewGuid(), "C", "D", "Info");

            Assert.NotEqual(Guid.Empty, capturedId1);
            Assert.NotEqual(Guid.Empty, capturedId2);
            Assert.NotEqual(capturedId1, capturedId2);
        }

        // ─── MarkAsReadAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task MarkAsReadAsync_ExistingNotification_SetsIsReadTrue()
        {
            var notificationId = Guid.NewGuid();
            var notification = new Notification { Id = notificationId, IsRead = false };
            _notificationRepositoryMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

            await _notificationService.MarkAsReadAsync(notificationId);

            Assert.True(notification.IsRead);
            _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_NotificationNotFound_DoesNotThrowAndDoesNotSave()
        {
            _notificationRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Notification)null);

            // Should gracefully do nothing
            var ex = await Record.ExceptionAsync(() => _notificationService.MarkAsReadAsync(Guid.NewGuid()));

            Assert.Null(ex);
            _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        // ─── GetUserNotificationsAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetUserNotificationsAsync_ReturnsUserNotifications()
        {
            var userId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "N1", IsRead = false },
                new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "N2", IsRead = true  }
            };

            _notificationRepositoryMock.Setup(r => r.GetUserNotificationsAsync(userId)).ReturnsAsync(notifications);

            var result = await _notificationService.GetUserNotificationsAsync(userId);

            Assert.Equal(2, result.Count);
            Assert.Equal("N1", result[0].Title);
            Assert.False(result[0].IsRead);
            Assert.True(result[1].IsRead);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_NoNotifications_ReturnsEmptyList()
        {
            _notificationRepositoryMock.Setup(r => r.GetUserNotificationsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Notification>());

            var result = await _notificationService.GetUserNotificationsAsync(Guid.NewGuid());

            Assert.Empty(result);
        }

        // ─── GetUnreadCountAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetUnreadCountAsync_ReturnsCorrectCount()
        {
            var userId = Guid.NewGuid();
            _notificationRepositoryMock.Setup(r => r.GetUnreadCountAsync(userId)).ReturnsAsync(7);

            var count = await _notificationService.GetUnreadCountAsync(userId);

            Assert.Equal(7, count);
        }

        [Fact]
        public async Task GetUnreadCountAsync_NoUnread_ReturnsZero()
        {
            _notificationRepositoryMock.Setup(r => r.GetUnreadCountAsync(It.IsAny<Guid>())).ReturnsAsync(0);

            var count = await _notificationService.GetUnreadCountAsync(Guid.NewGuid());

            Assert.Equal(0, count);
        }
    }
}
