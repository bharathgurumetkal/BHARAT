using Insurance.Application.DTOs.Policy;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;


namespace Insurance.Application.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _policyRepository;
    private readonly IPropertyRepository _propertyRepository;

    public PolicyService(
        IPolicyRepository policyRepository,
        IPropertyRepository propertyRepository)
    {
        _policyRepository = policyRepository;
        _propertyRepository = propertyRepository;
    }

    public async Task<Guid> CreatePolicyAsync(CreatePolicyDto dto, Guid adminId)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Category = dto.PropertyCategory,
            SubCategory = dto.PropertySubCategory,
            Address = dto.Address,
            YearBuilt = dto.YearBuilt,
            MarketValue = dto.MarketValue,
            RiskZone = dto.RiskZone,
            HasSecuritySystem = dto.HasSecuritySystem
        };

        await _propertyRepository.AddAsync(property);

        var premium = CalculatePremium(dto);

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyNumber = GeneratePolicyNumber(),
            CustomerId = dto.CustomerId,
            PropertyId = property.Id,
            CreatedByAdminId = adminId,  // 🔥 NEW
            CoverageAmount = dto.CoverageAmount,
            Premium = premium,
            Status = PolicyStatus.Draft
        };

        await _policyRepository.AddPolicyAsync(policy);
        await _policyRepository.SaveChangesAsync();

        return policy.Id;
    }

    private decimal CalculatePremium(CreatePolicyDto dto)
    {
        decimal baseRate = dto.CoverageAmount * 0.02m;

        if (dto.RiskZone == "High")
            baseRate += dto.CoverageAmount * 0.01m;

        if (!dto.HasSecuritySystem)
            baseRate += 500;

        return baseRate;
    }

    private string GeneratePolicyNumber()
    {
        return $"POL-{DateTime.UtcNow.Ticks}";
    }

    public async Task<List<PolicyDto>> GetPoliciesByCustomerAsync(Guid customerUserId)
    {
        var policies = await _policyRepository.GetPoliciesByCustomerAsync(customerUserId);
        return policies.Select(p => new PolicyDto
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            CustomerId = p.CustomerId,
            PropertyId = p.PropertyId,
            CoverageAmount = p.CoverageAmount,
            Premium = p.Premium,
            Status = p.Status.ToString(),
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            ApplicationId = p.ApplicationId,
            ProductName = p.Application?.Product?.Name ?? "General Insurance"
        }).ToList();
    }

    public async Task<List<PolicyDto>> GetAllPoliciesAsync()
    {
        var policies = await _policyRepository.GetAllAsync();
        return policies.Select(p => new PolicyDto
        {
            Id = p.Id,
            PolicyNumber = p.PolicyNumber,
            CustomerId = p.CustomerId,
            PropertyId = p.PropertyId,
            CoverageAmount = p.CoverageAmount,
            Premium = p.Premium,
            Status = p.Status.ToString(),
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            ApplicationId = p.ApplicationId,
            ProductName = p.Application?.Product?.Name ?? "General Insurance"
        }).ToList();
    }
}