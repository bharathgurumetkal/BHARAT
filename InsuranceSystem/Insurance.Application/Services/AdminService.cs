using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services;

public class AdminService : IAdminService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly INotificationService _notificationService;

    public AdminService(
        ICustomerRepository customerRepository,
        INotificationService notificationService)
    {
        _customerRepository = customerRepository;
        _notificationService = notificationService;
    }

    public async Task AssignCustomerAsync(Guid customerId, Guid agentId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);

        if (customer == null)
            throw new Exception("Customer not found.");

        customer.AssignedAgentId = agentId;
        customer.Status = "Assigned";

        await _customerRepository.SaveChangesAsync();

        await _notificationService.CreateAsync(
            agentId,
            "New Customer Assigned",
            $"The customer {customer.User?.Name ?? "New Client"} has been assigned to you.",
            "Info"
        );
    }
    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllAsync();
    }
}