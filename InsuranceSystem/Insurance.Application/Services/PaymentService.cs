using Insurance.Application.DTOs.Payment;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;


namespace Insurance.Application.Services;

public class PaymentService : IPaymentService
{
    private const decimal CommissionRate = 0.10m; // 10% configurable constant

    private readonly IPolicyRepository _policyRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly INotificationService _notificationService;
    private readonly ICommissionRepository _commissionRepository;
    private readonly IPolicyApplicationRepository _applicationRepository;

    public PaymentService(
        IPolicyRepository policyRepository,
        IPaymentRepository paymentRepository,
        INotificationService notificationService,
        ICommissionRepository commissionRepository,
        IPolicyApplicationRepository applicationRepository)
    {
        _policyRepository = policyRepository;
        _paymentRepository = paymentRepository;
        _notificationService = notificationService;
        _commissionRepository = commissionRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task ProcessPaymentAsync(MakePaymentDto dto)
    {
        var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);

        if (policy == null)
            throw new Exception("Policy not found.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PolicyId = dto.PolicyId,
            Amount = dto.Amount,
            PaymentDate = DateTime.UtcNow,
            Status = "Completed"
        };

        await _paymentRepository.AddAsync(payment);

        policy.Status = PolicyStatus.Active;
        policy.StartDate = DateTime.UtcNow;
        policy.EndDate = policy.StartDate.Value.AddYears(1);

        await _paymentRepository.SaveChangesAsync();

        // ── Commission Logic ────────────────────────────────────────────────────
        // Commission is only generated for policies that came from the application
        // workflow AND have an assigned agent. Duplicate-guarded.
        if (policy.ApplicationId.HasValue)
        {
            var alreadyExists = await _commissionRepository.ExistsForPolicyAsync(policy.Id);
            if (!alreadyExists)
            {
                var application = await _applicationRepository.GetByIdAsync(policy.ApplicationId.Value);

                if (application?.AssignedAgentId != null)
                {
                    var commission = new Commission
                    {
                        Id = Guid.NewGuid(),
                        AgentId = application.AssignedAgentId.Value,
                        PolicyId = policy.Id,
                        CommissionRate = CommissionRate,
                        CommissionAmount = Math.Round(policy.Premium * CommissionRate, 2),
                        CreatedAt = DateTime.UtcNow,
                        IsPaid = false
                    };

                    await _commissionRepository.AddAsync(commission);
                    await _commissionRepository.SaveChangesAsync();
                }
            }
        }
        // ── End Commission Logic ────────────────────────────────────────────────

        await _notificationService.CreateAsync(
            policy.CustomerId,
            "Policy Activated",
            $"Your policy {policy.PolicyNumber} is now Active. Coverage starts today.",
            "Success"
        );
    }
}