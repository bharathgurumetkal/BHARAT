using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task SaveChangesAsync();
}