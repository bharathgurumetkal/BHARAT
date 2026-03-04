using Insurance.Application.DTOs.Payment;

namespace Insurance.Application.Interfaces;

public interface IPaymentService
{
    Task ProcessPaymentAsync(MakePaymentDto dto);
}