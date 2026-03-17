using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Application.DTOs.AuditLog;

namespace Insurance.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(AuditLogEntry entry);
        Task<List<AuditLogDto>> GetAllLogsAsync();
    }
}
