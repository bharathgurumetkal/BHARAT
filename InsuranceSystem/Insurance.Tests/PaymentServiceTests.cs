using System;
using System.Threading.Tasks;
using Insurance.Application.DTOs.Payment;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class PaymentServiceTests
    {
        private Mock<IPolicyRepository> _policyRepositoryMock;
        private Mock<IPaymentRepository> _paymentRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<ICommissionRepository> _commissionRepositoryMock;
        private Mock<IPolicyApplicationRepository> _applicationRepositoryMock;
        private PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _commissionRepositoryMock = new Mock<ICommissionRepository>();
            _applicationRepositoryMock = new Mock<IPolicyApplicationRepository>();

            _paymentService = new PaymentService(
                _policyRepositoryMock.Object,
                _paymentRepositoryMock.Object,
                _notificationServiceMock.Object,
                _commissionRepositoryMock.Object,
                _applicationRepositoryMock.Object
            );
        }

        // ─── ProcessPaymentAsync – Core ───────────────────────────────────────────

        [Fact]
        public async Task ProcessPaymentAsync_ValidDraftPolicy_ActivatesPolicyAndNotifiesCustomer()
        {
            var policyId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, CustomerId = customerId, PolicyNumber = "POL-001", Status = PolicyStatus.Draft };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 1000 });

            Assert.Equal(PolicyStatus.Active, policy.Status);
            _notificationServiceMock.Verify(n => n.CreateAsync(customerId, "Policy Activated", It.IsAny<string>(), "Success"), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_SetsStartDateAndEndDateOneYearApart()
        {
            var policyId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, Status = PolicyStatus.Draft };
            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            var beforeCall = DateTime.UtcNow;
            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 500 });

            Assert.NotNull(policy.StartDate);
            Assert.NotNull(policy.EndDate);
            Assert.True(policy.StartDate >= beforeCall);
            Assert.Equal(policy.StartDate.Value.AddYears(1), policy.EndDate.Value);
        }

        [Fact]
        public async Task ProcessPaymentAsync_RecordsPaymentInRepository()
        {
            var policyId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, Status = PolicyStatus.Draft };
            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 2500 });

            _paymentRepositoryMock.Verify(r => r.AddAsync(It.Is<Payment>(p =>
                p.PolicyId == policyId &&
                p.Amount == 2500 &&
                p.Status == "Completed"
            )), Times.Once);
            _paymentRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_PolicyNotFound_ThrowsException()
        {
            _policyRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Policy)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = Guid.NewGuid(), Amount = 100 }));

            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ─── Commission Logic ─────────────────────────────────────────────────────

        [Fact]
        public async Task ProcessPaymentAsync_WithApplicationAndAgent_GeneratesCorrectCommission()
        {
            var policyId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, ApplicationId = appId, Premium = 5000m, Status = PolicyStatus.Draft };
            var application = new PolicyApplication { Id = appId, AssignedAgentId = agentId };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _commissionRepositoryMock.Setup(r => r.ExistsForPolicyAsync(policyId)).ReturnsAsync(false);
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);

            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 5000 });

            // Commission = 5000 * 10% = 500
            _commissionRepositoryMock.Verify(r => r.AddAsync(It.Is<Commission>(c =>
                c.AgentId == agentId &&
                c.CommissionAmount == 500m &&
                c.CommissionRate == 0.10m &&
                c.IsPaid == false
            )), Times.Once);
            _commissionRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_DuplicatePayment_DoesNotGenerateCommissionAgain()
        {
            // Guard: if commission already exists for this policy, skip it
            var policyId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, ApplicationId = appId, Premium = 3000m, Status = PolicyStatus.Draft };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _commissionRepositoryMock.Setup(r => r.ExistsForPolicyAsync(policyId)).ReturnsAsync(true); // Already exists

            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 3000 });

            _commissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Commission>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentAsync_NoApplicationLink_SkipsCommission()
        {
            // Policies manually created by admin have no ApplicationId → no commission
            var policyId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, ApplicationId = null, Premium = 4000m, Status = PolicyStatus.Draft };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 4000 });

            _commissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Commission>()), Times.Never);
            _commissionRepositoryMock.Verify(r => r.ExistsForPolicyAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ApplicationWithNoAgent_SkipsCommission()
        {
            // Application exists but has no assigned agent → no commission generated
            var policyId = Guid.NewGuid();
            var appId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, ApplicationId = appId, Premium = 2000m, Status = PolicyStatus.Draft };
            var application = new PolicyApplication { Id = appId, AssignedAgentId = null };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _commissionRepositoryMock.Setup(r => r.ExistsForPolicyAsync(policyId)).ReturnsAsync(false);
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);

            await _paymentService.ProcessPaymentAsync(new MakePaymentDto { PolicyId = policyId, Amount = 2000 });

            _commissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Commission>()), Times.Never);
        }
    }
}
