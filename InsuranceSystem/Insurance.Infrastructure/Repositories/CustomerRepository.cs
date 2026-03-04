using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<Customer?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<List<Customer>> GetByAgentIdAsync(Guid agentId)
    {
        var applicationAssignedUserIds = _context.PolicyApplications
            .Where(a => a.AssignedAgentId == agentId)
            .Select(a => a.CustomerId);

        return await _context.Customers
            .Include(c => c.User)
            .Where(c => c.AssignedAgentId == agentId || applicationAssignedUserIds.Contains(c.UserId))
            .ToListAsync();
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}