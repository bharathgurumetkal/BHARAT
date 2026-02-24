using System;
using Insurance.Application.Interfaces;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Application.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AssignCustomerAsync(Guid customerId, Guid agentId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new Exception("Customer not found.");

        customer.AssignedAgentId = agentId;
        customer.Status = "Assigned";

        await _context.SaveChangesAsync();
    }
}