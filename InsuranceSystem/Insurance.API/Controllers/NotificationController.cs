using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Insurance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { count });
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }
    }
}
