namespace Insurance.Application.DTOs.Policy;

public class CreatePolicyDto
{
    public Guid CustomerId { get; set; }

    public string PropertyCategory { get; set; } = default!;
    public string PropertySubCategory { get; set; } = default!;
    public string Address { get; set; } = default!;
    public int YearBuilt { get; set; }
    public decimal MarketValue { get; set; }
    public string RiskZone { get; set; } = default!;
    public bool HasSecuritySystem { get; set; }

    public decimal CoverageAmount { get; set; }
}