using System;

namespace Insurance.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }   // Target user
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Type { get; set; } = "Info";     // Info, Success, Warning, Risk
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
