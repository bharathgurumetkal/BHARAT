using System;

namespace Insurance.Application.DTOs.AuditLog
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserRole { get; set; }
        public string Action { get; set; } = default!;
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Description { get; set; } = default!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
