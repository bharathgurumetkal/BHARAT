using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class ClaimRepository : IClaimRepository
{
    private readonly AppDbContext _context;

    public ClaimRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Claim claim)
    {
        await _context.Claims.AddAsync(claim);
    }

    public async Task AddDocumentAsync(ClaimDocument document)
    {
        await _context.ClaimDocuments.AddAsync(document);
    }

    public async Task<Claim?> GetByIdAsync(Guid id)
    {
        return await _context.Claims
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    public async Task<List<Claim>> GetClaimsByCustomerAsync(Guid customerUserId)
    {
        return await _context.Claims
            .AsNoTracking()
            .Include(c => c.Documents)
            .Include(c => c.Policy)
            .ThenInclude(p => p.Customer)
            .Include(c => c.Policy)
            .ThenInclude(p => p.Property)
            .Include(c => c.Policy)
            .ThenInclude(p => p.Application)
                .ThenInclude(a => a.Product)
            .Where(c => c.Policy.Customer.UserId == customerUserId)
            .ToListAsync();
    }

    public async Task<List<Claim>> GetAllAsync()
    {
        return await _context.Claims
            .AsNoTracking()
            .Include(c => c.Documents)
            .Include(c => c.Policy)
            .ThenInclude(p => p.Customer)
            .ThenInclude(u => u.User)
            .Include(c => c.Policy)
            .ThenInclude(p => p.Property)
            .Include(c => c.Policy)
            .ThenInclude(p => p.Application)
                .ThenInclude(a => a.Product)
            // Risk-Based Priority Queue: scored claims first (highest risk first), unscored last
            .OrderByDescending(c => c.AiRiskScore.HasValue ? 1 : 0)
            .ThenByDescending(c => c.AiRiskScore ?? 0)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}