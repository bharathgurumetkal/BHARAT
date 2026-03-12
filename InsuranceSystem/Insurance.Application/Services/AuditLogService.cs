using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Insurance.Application.DTOs.AuditLog;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IAuditLogRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(AuditLogEntry entry)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var request = _httpContextAccessor.HttpContext?.Request;
            var connection = _httpContextAccessor.HttpContext?.Connection;

            var userId = entry.UserId ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            var role = entry.UserRole ?? user?.FindFirst(ClaimTypes.Role)?.Value ?? "System";
            var email = entry.UserEmail ?? user?.FindFirst(ClaimTypes.Email)?.Value;

            var ipAddress = connection?.RemoteIpAddress?.ToString();
            var userAgent = request?.Headers["User-Agent"].ToString();

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = entry.Action,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                UserId = userId,
                UserRole = role,
                UserEmail = email,
                OldValue = entry.OldValue,
                NewValue = entry.NewValue,
                CreatedAt = DateTime.UtcNow,
                Description = entry.Description,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            try 
            {
                await _repository.AddAsync(log);
            }
            catch (Exception ex)
            {
                // Never crash the main request because logging failed
                Console.WriteLine($"[AuditLogService] FAILED to save log: {ex.Message}");
            }
        }

        public async Task<List<AuditLogDto>> GetAllLogsAsync()
        {
            var logs = await _repository.GetAllAsync();
            return logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                UserId = l.UserId,
                UserRole = l.UserRole,
                UserEmail = l.UserEmail,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                // EF Core returns DateTime with Kind=Unspecified from SQL Server.
                // SpecifyKind forces UTC so System.Text.Json serializes with 'Z',
                // ensuring the browser parses it as UTC and displays IST correctly.
                CreatedAt = DateTime.SpecifyKind(l.CreatedAt, DateTimeKind.Utc),
                Description = l.Description,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent
            }).ToList();
        }
    }
}
