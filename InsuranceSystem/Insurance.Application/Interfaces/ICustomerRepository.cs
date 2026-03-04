using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByUserIdAsync(Guid userId);
        Task<List<Customer>> GetByAgentIdAsync(Guid agentId);
        Task AddAsync(Customer customer);

        Task SaveChangesAsync();
    }
}
