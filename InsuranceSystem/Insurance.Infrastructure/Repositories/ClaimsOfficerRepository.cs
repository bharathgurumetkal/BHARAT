using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public class ClaimsOfficerRepository : IClaimsOfficerRepository
{
    private readonly AppDbContext _context;

    public ClaimsOfficerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ClaimsOfficer officer)
    {
        await _context.ClaimsOfficers.AddAsync(officer);
    }

    public async Task<List<ClaimsOfficer>> GetAllAsync()
    {
        return await _context.ClaimsOfficers.Include(o => o.User).ToListAsync();
    }
}