using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Insurance.Application.DTOs.PolicyApplication;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class PolicyApplicationServiceTests
    {
        private Mock<IPolicyApplicationRepository> _applicationRepositoryMock;
        private Mock<IPolicyProductRepository> _productRepositoryMock;
        private Mock<IPolicyRepository> _policyRepositoryMock;
        private Mock<IPropertyRepository> _propertyRepositoryMock;
        private Mock<ICustomerRepository> _customerRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private PolicyApplicationService _applicationService;

        public PolicyApplicationServiceTests()
        {
            _applicationRepositoryMock = new Mock<IPolicyApplicationRepository>();
            _productRepositoryMock = new Mock<IPolicyProductRepository>();
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            _applicationService = new PolicyApplicationService(
                _applicationRepositoryMock.Object,
                _productRepositoryMock.Object,
                _policyRepositoryMock.Object,
                _propertyRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _notificationServiceMock.Object
            );
        }

        // ─── ApplyForProductAsync ────────────────────────────────────────────────

        [Fact]
        public async Task ApplyForProductAsync_ValidProduct_CalculatesPremiumAndSubmits()
        {
            // Premium = 100,000 * 2% = 2,000
            var productId = Guid.NewGuid();
            var customerUserId = Guid.NewGuid();
            var product = new PolicyProduct { Id = productId, Name = "Home Insurance", IsActive = true, MaxCoverageAmount = 500_000, BaseRatePercentage = 2.0m };
            var dto = new ApplyForProductDto { ProductId = productId, RequestedCoverageAmount = 100_000, RiskZone = "Low", HasSecuritySystem = true };

            _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            await _applicationService.ApplyForProductAsync(customerUserId, dto);

            _applicationRepositoryMock.Verify(r => r.AddAsync(It.Is<PolicyApplication>(a =>
                a.CalculatedPremium == 2000m &&
                a.Status == "Submitted" &&
                a.CustomerId == customerUserId
            )), Times.Once);
            _applicationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApplyForProductAsync_ProductNotFound_ThrowsException()
        {
            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PolicyProduct)null);
            var dto = new ApplyForProductDto { ProductId = Guid.NewGuid(), RequestedCoverageAmount = 10_000 };

            await Assert.ThrowsAsync<Exception>(() => _applicationService.ApplyForProductAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task ApplyForProductAsync_ExceedsMaxCoverage_ThrowsException()
        {
            var productId = Guid.NewGuid();
            var product = new PolicyProduct { Id = productId, IsActive = true, MaxCoverageAmount = 100_000, BaseRatePercentage = 2m };
            var dto = new ApplyForProductDto { ProductId = productId, RequestedCoverageAmount = 200_000 }; // exceeds max

            _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            await Assert.ThrowsAsync<Exception>(() => _applicationService.ApplyForProductAsync(Guid.NewGuid(), dto));
        }

        // ─── AssignAgentAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task AssignAgentAsync_ValidSubmission_UpdatesStatusAndCustomerAssignment()
        {
            var appId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var application = new PolicyApplication { Id = appId, Status = "Submitted", CustomerId = customerId };
            var customer = new Customer { UserId = customerId };

            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);
            _customerRepositoryMock.Setup(r => r.GetByUserIdAsync(customerId)).ReturnsAsync(customer);

            await _applicationService.AssignAgentAsync(appId, agentId);

            Assert.Equal("AssignedToAgent", application.Status);
            Assert.Equal(agentId, application.AssignedAgentId);
            Assert.NotNull(application.AssignedAt);
            Assert.Equal(agentId, customer.AssignedAgentId);
            _applicationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignAgentAsync_ApplicationNotFound_ThrowsException()
        {
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PolicyApplication)null);

            await Assert.ThrowsAsync<Exception>(() => _applicationService.AssignAgentAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        // ─── ApproveApplicationAsync ──────────────────────────────────────────────

        [Fact]
        public async Task ApproveApplicationAsync_ByAssignedAgent_CreatesDraftPolicy()
        {
            var appId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customerUserId = Guid.NewGuid();
            var customer = new Customer { Id = Guid.NewGuid(), UserId = customerUserId };
            var application = new PolicyApplication
            {
                Id = appId, Status = "AssignedToAgent", AssignedAgentId = agentId,
                CustomerId = customerUserId, CalculatedPremium = 3000, RequestedCoverageAmount = 150_000
            };

            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);
            _customerRepositoryMock.Setup(r => r.GetByUserIdAsync(customerUserId)).ReturnsAsync(customer);

            await _applicationService.ApproveApplicationAsync(appId, agentId);

            Assert.Equal("ApprovedByAgent", application.Status);
            Assert.NotNull(application.ReviewedAt);
            _propertyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Property>()), Times.Once);
            _policyRepositoryMock.Verify(r => r.AddPolicyAsync(It.Is<Policy>(p =>
                p.Status == Insurance.Domain.Enums.PolicyStatus.Draft &&
                p.Premium == 3000m &&
                p.CoverageAmount == 150_000m
            )), Times.Once);
            _notificationServiceMock.Verify(n => n.CreateAsync(customerUserId, "Application Approved", It.IsAny<string>(), "Success"), Times.Once);
        }

        [Fact]
        public async Task ApproveApplicationAsync_WrongAgent_ThrowsException()
        {
            var appId = Guid.NewGuid();
            var assignedAgentId = Guid.NewGuid();
            var differentAgentId = Guid.NewGuid();
            var application = new PolicyApplication
            {
                Id = appId, Status = "AssignedToAgent",
                AssignedAgentId = assignedAgentId, CustomerId = Guid.NewGuid()
            };

            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);
            _customerRepositoryMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer)null);

            await Assert.ThrowsAsync<Exception>(() => _applicationService.ApproveApplicationAsync(appId, differentAgentId));
        }

        // ─── RejectApplicationAsync ───────────────────────────────────────────────

        [Fact]
        public async Task RejectApplicationAsync_ValidAgent_UpdatesStatusToRejected()
        {
            var appId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var application = new PolicyApplication { Id = appId, Status = "AssignedToAgent", AssignedAgentId = agentId, CustomerId = customerId };

            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);

            await _applicationService.RejectApplicationAsync(appId, agentId);

            Assert.Equal("RejectedByAgent", application.Status);
            Assert.NotNull(application.ReviewedAt);
            _applicationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _notificationServiceMock.Verify(n => n.CreateAsync(customerId, "Application Rejected", It.IsAny<string>(), "Risk"), Times.Once);
        }

        [Fact]
        public async Task RejectApplicationAsync_ApplicationNotFound_ThrowsException()
        {
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PolicyApplication)null);

            await Assert.ThrowsAsync<Exception>(() => _applicationService.RejectApplicationAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        // ─── GetAllApplicationsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetAllApplicationsAsync_ReturnsMappedDtos()
        {
            var agentId = Guid.NewGuid();
            var applications = new List<PolicyApplication>
            {
                new PolicyApplication { Id = Guid.NewGuid(), Status = "Submitted",      SubmittedAt = DateTime.UtcNow },
                new PolicyApplication { Id = Guid.NewGuid(), Status = "AssignedToAgent", SubmittedAt = DateTime.UtcNow }
            };

            _applicationRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(applications);

            var result = await _applicationService.GetAllApplicationsAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Submitted",       result[0].Status);
            Assert.Equal("AssignedToAgent", result[1].Status);
        }

        [Fact]
        public async Task GetApplicationsByCustomerAsync_ReturnsMappedDtos()
        {
            var customerId = Guid.NewGuid();
            var applications = new List<PolicyApplication>
            {
                new PolicyApplication { Id = Guid.NewGuid(), CustomerId = customerId, Status = "Submitted" }
            };

            _applicationRepositoryMock.Setup(r => r.GetByCustomerIdAsync(customerId)).ReturnsAsync(applications);

            var result = await _applicationService.GetApplicationsByCustomerAsync(customerId);

            Assert.Single(result);
            Assert.Equal(customerId, result[0].CustomerId);
        }
    }
}
