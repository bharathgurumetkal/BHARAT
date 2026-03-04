using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class PolicyApplicationRepository : IPolicyApplicationRepository
    {
        private readonly AppDbContext _context;

        public PolicyApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PolicyApplication application)
        {
            await _context.PolicyApplications.AddAsync(application);
        }

        public async Task<PolicyApplication?> GetByIdAsync(Guid id)
        {
            return await _context.PolicyApplications
                .Include(a => a.Product)
                .Include(a => a.Customer)
                .Include(a => a.AssignedAgent)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<PolicyApplication>> GetByAgentIdAsync(Guid agentUserId)
        {
            // We want applications that are:
            // 1. Directly assigned to this agent (any status, so they see history)
            // 2. OR: Unassigned (Submitted) but the customer belongs to this agent
            return await _context.PolicyApplications
                .Include(a => a.Product)
                .Include(a => a.Customer)
                .Include(a => a.AssignedAgent)
                .Where(a => a.AssignedAgentId == agentUserId || 
                           (a.AssignedAgentId == null && a.Status == "Submitted" &&
                            _context.Customers.Any(c => c.UserId == a.CustomerId && c.AssignedAgentId == agentUserId)))
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<PolicyApplication>> GetByCustomerIdAsync(Guid customerUserId)
        {
            return await _context.PolicyApplications
                .Include(a => a.Product)
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == customerUserId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<PolicyApplication>> GetAllAsync()
        {
            return await _context.PolicyApplications
                .Include(a => a.Product)
                .Include(a => a.Customer)
                .Include(a => a.AssignedAgent)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
