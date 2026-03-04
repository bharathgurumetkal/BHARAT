using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetByRoleAsync(string role)
    {
        if (Enum.TryParse<RoleType>(role, out var roleType))
        {
            return await _context.Users.Where(u => u.Role == roleType).ToListAsync();
        }
        return new List<User>();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}