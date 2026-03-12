using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly AppDbContext _context;

    public PolicyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddPolicyAsync(Policy policy)
    {
        await _context.Policies.AddAsync(policy);
    }

    public async Task<Policy?> GetByIdAsync(Guid id)
    {
        return await _context.Policies
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Policy>> GetPoliciesByAgentAsync(Guid agentUserId)
    {
        return await _context.Policies
            .Include(p => p.Customer)
                .ThenInclude(c => c.User)
            .Where(p => p.Customer.AssignedAgentId == agentUserId || p.CreatedByAdminId == agentUserId)
            .ToListAsync();
    }

    public async Task<List<Policy>> GetPoliciesByCustomerAsync(Guid customerUserId)
    {
        return await _context.Policies
            .Include(p => p.Customer)
            .Include(p => p.Application)
                .ThenInclude(a => a.Product)
            .Where(p => p.Customer.UserId == customerUserId)
            .ToListAsync();
    }

    public async Task<List<Policy>> GetAllAsync()
    {
        return await _context.Policies
            .Include(p => p.Customer)
            .ThenInclude(c => c.User)
            .Include(p => p.Property)
            .Include(p => p.Application)
                .ThenInclude(a => a.Product)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}