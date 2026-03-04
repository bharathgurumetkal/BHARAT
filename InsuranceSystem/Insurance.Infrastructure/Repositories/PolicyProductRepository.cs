using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class PolicyProductRepository : IPolicyProductRepository
    {
        private readonly AppDbContext _context;

        public PolicyProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PolicyProduct product)
        {
            await _context.PolicyProducts.AddAsync(product);
        }

        public async Task<PolicyProduct?> GetByIdAsync(Guid id)
        {
            return await _context.PolicyProducts.FindAsync(id);
        }

        public async Task<List<PolicyProduct>> GetAllActiveAsync()
        {
            return await _context.PolicyProducts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
