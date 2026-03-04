using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly AppDbContext _context;

    public AgentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Agent agent)
    {
        await _context.Agents.AddAsync(agent);
    }

    public async Task<List<Agent>> GetAllAsync()
    {
        return await _context.Agents.Include(a => a.User).ToListAsync();
    }
}