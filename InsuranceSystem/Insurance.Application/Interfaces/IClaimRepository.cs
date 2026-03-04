using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IClaimRepository
    {
        Task AddAsync(Claim claim);
        Task AddDocumentAsync(ClaimDocument document);
        Task<Claim?> GetByIdAsync(Guid id);
        Task<List<Claim>> GetClaimsByCustomerAsync(Guid customerUserId);
        Task<List<Claim>> GetAllAsync();
        Task SaveChangesAsync();
    }
}
