using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetByRoleAsync(string role);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
