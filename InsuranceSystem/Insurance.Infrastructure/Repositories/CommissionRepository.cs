using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly AppDbContext _context;

        public CommissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Commission commission)
        {
            await _context.Commissions.AddAsync(commission);
        }

        /// <summary>
        /// Guard against duplicate commissions for the same policy.
        /// </summary>
        public async Task<bool> ExistsForPolicyAsync(Guid policyId)
        {
            return await _context.Commissions.AnyAsync(c => c.PolicyId == policyId);
        }

        /// <summary>
        /// Returns all commissions for an agent, with policy details, newest first.
        /// </summary>
        public async Task<List<Commission>> GetByAgentIdAsync(Guid agentUserId)
        {
            return await _context.Commissions
                .Include(c => c.Policy)
                    .ThenInclude(p => p.Customer)
                        .ThenInclude(cu => cu.User)
                .Where(c => c.AgentId == agentUserId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
