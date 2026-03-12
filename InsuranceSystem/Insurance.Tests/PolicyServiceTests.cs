using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Application.DTOs.Policy;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class PolicyServiceTests
    {
        private Mock<IPolicyRepository> _policyRepositoryMock;
        private Mock<IPropertyRepository> _propertyRepositoryMock;
        private Mock<IAuditLogService> _auditLogServiceMock;
        private PolicyService _policyService;

        public PolicyServiceTests()
        {
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _auditLogServiceMock = new Mock<IAuditLogService>();
            _policyService = new PolicyService(_policyRepositoryMock.Object, _propertyRepositoryMock.Object, _auditLogServiceMock.Object);
        }

        // ─── Premium Calculation ─────────────────────────────────────────────────

        [Fact]
        public async Task CreatePolicyAsync_LowRiskWithSecurity_CalculatesBasePremium()
        {
            // Base: 100,000 * 2% = 2,000
            var dto = BuildDto(riskZone: "Low", hasSecurity: true, coverage: 100_000);

            await _policyService.CreatePolicyAsync(dto, Guid.NewGuid());

            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p => p.Premium == 2000m)), Times.Once);
        }

        [Fact]
        public async Task CreatePolicyAsync_HighRiskWithoutSecurity_AddsAllSurcharges()
        {
            // Base: 100,000 * 2% = 2,000
            // High-risk surcharge: +100,000 * 1% = 1,000
            // No-security surcharge: +500
            // Total: 3,500
            var dto = BuildDto(riskZone: "High", hasSecurity: false, coverage: 100_000);

            await _policyService.CreatePolicyAsync(dto, Guid.NewGuid());

            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p => p.Premium == 3500m)), Times.Once);
        }

        [Fact]
        public async Task CreatePolicyAsync_LowRiskWithoutSecurity_AddsOnlyNoSecuritySurcharge()
        {
            // Base: 100,000 * 2% = 2,000
            // No-security surcharge: +500
            // Total: 2,500
            var dto = BuildDto(riskZone: "Low", hasSecurity: false, coverage: 100_000);

            await _policyService.CreatePolicyAsync(dto, Guid.NewGuid());

            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p => p.Premium == 2500m)), Times.Once);
        }

        [Fact]
        public async Task CreatePolicyAsync_HighRiskWithSecurity_AddsOnlyHighRiskSurcharge()
        {
            // Base: 200,000 * 2% = 4,000
            // High-risk surcharge: +200,000 * 1% = 2,000
            // Total: 6,000
            var dto = BuildDto(riskZone: "High", hasSecurity: true, coverage: 200_000);

            await _policyService.CreatePolicyAsync(dto, Guid.NewGuid());

            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p => p.Premium == 6000m)), Times.Once);
        }

        // ─── Status & Metadata ───────────────────────────────────────────────────

        [Fact]
        public async Task CreatePolicyAsync_AlwaysCreatesPolicyInDraftStatus()
        {
            var dto = BuildDto();

            await _policyService.CreatePolicyAsync(dto, Guid.NewGuid());

            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p => p.Status == PolicyStatus.Draft)), Times.Once);
        }

        [Fact]
        public async Task CreatePolicyAsync_StoresAdminId()
        {
            var adminId = Guid.NewGuid();
            var dto = BuildDto();

            await _policyService.CreatePolicyAsync(dto, adminId);

            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p => p.CreatedByAdminId == adminId)), Times.Once);
        }

        [Fact]
        public async Task CreatePolicyAsync_AlsoCreatesProperty()
        {
            var dto = BuildDto(riskZone: "Low", hasSecurity: true, coverage: 50_000);

            await _policyService.CreatePolicyAsync(dto, Guid.NewGuid());

            _propertyRepositoryMock.Verify(r => r.AddAsync(It.Is<Property>(p =>
                p.RiskZone == dto.RiskZone &&
                p.HasSecuritySystem == dto.HasSecuritySystem
            )), Times.Once);
        }

        [Fact]
        public async Task CreatePolicyAsync_SavesChanges()
        {
            await _policyService.CreatePolicyAsync(BuildDto(), Guid.NewGuid());

            _policyRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ─── Query Methods ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetPoliciesByCustomerAsync_ReturnsMappedDtos()
        {
            var customerId = Guid.NewGuid();
            var policies = new List<Policy>
            {
                new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL-001", CustomerId = customerId, Status = PolicyStatus.Active, Premium = 1500, CoverageAmount = 75000 },
                new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL-002", CustomerId = customerId, Status = PolicyStatus.Draft,  Premium = 2000, CoverageAmount = 100000 }
            };

            _policyRepositoryMock.Setup(r => r.GetPoliciesByCustomerAsync(customerId)).ReturnsAsync(policies);

            var result = await _policyService.GetPoliciesByCustomerAsync(customerId);

            Assert.Equal(2, result.Count);
            Assert.Equal("Active", result[0].Status);
            Assert.Equal("Draft",  result[1].Status);
            Assert.Equal("POL-001", result[0].PolicyNumber);
        }

        [Fact]
        public async Task GetAllPoliciesAsync_ReturnsMappedDtos()
        {
            var policies = new List<Policy>
            {
                new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL-100", Status = PolicyStatus.Active },
                new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL-101", Status = PolicyStatus.Expired }
            };

            _policyRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(policies);

            var result = await _policyService.GetAllPoliciesAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("POL-100", result[0].PolicyNumber);
            Assert.Equal("Expired", result[1].Status);
        }

        [Fact]
        public async Task GetPoliciesByCustomerAsync_EmptyList_ReturnsEmpty()
        {
            _policyRepositoryMock.Setup(r => r.GetPoliciesByCustomerAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Policy>());

            var result = await _policyService.GetPoliciesByCustomerAsync(Guid.NewGuid());

            Assert.Empty(result);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static CreatePolicyDto BuildDto(
            string riskZone = "Low", bool hasSecurity = true, decimal coverage = 100_000)
        {
            return new CreatePolicyDto
            {
                CoverageAmount = coverage,
                RiskZone = riskZone,
                HasSecuritySystem = hasSecurity,
                Address = "1 Test Street",
                PropertyCategory = "Residential",
                PropertySubCategory = "Villa",
                YearBuilt = 2010,
                MarketValue = 200_000
            };
        }
    }
}
