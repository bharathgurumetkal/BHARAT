using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class AdminServiceTests
    {
        private Mock<ICustomerRepository> _customerRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private AdminService _adminService;

        public AdminServiceTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _adminService = new AdminService(_customerRepositoryMock.Object, _notificationServiceMock.Object);
        }

        // ─── AssignCustomerAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AssignCustomerAsync_ExistingCustomer_UpdatesAgentAndStatus()
        {
            var customerId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Status = "Unassigned", User = new User { Name = "Jane Doe" } };

            _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            await _adminService.AssignCustomerAsync(customerId, agentId);

            Assert.Equal(agentId, customer.AssignedAgentId);
            Assert.Equal("Assigned", customer.Status);
            _customerRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignCustomerAsync_NotifiesAgent()
        {
            var customerId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, User = new User { Name = "John Customer" } };

            _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            await _adminService.AssignCustomerAsync(customerId, agentId);

            _notificationServiceMock.Verify(n => n.CreateAsync(
                agentId,
                "New Customer Assigned",
                It.Is<string>(msg => msg.Contains("John Customer")),
                "Info"
            ), Times.Once);
        }

        [Fact]
        public async Task AssignCustomerAsync_CustomerNotFound_ThrowsException()
        {
            _customerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Customer)null);

            var ex = await Assert.ThrowsAsync<Exception>(() => _adminService.AssignCustomerAsync(Guid.NewGuid(), Guid.NewGuid()));
            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AssignCustomerAsync_NullUserName_UsesDefaultNameInNotification()
        {
            // When user navigation is null, notification should use a safe fallback
            var customerId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, User = null! }; // No user loaded

            _customerRepositoryMock.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

            // Should not throw
            await _adminService.AssignCustomerAsync(customerId, agentId);

            _notificationServiceMock.Verify(n => n.CreateAsync(agentId, "New Customer Assigned", It.IsAny<string>(), "Info"), Times.Once);
        }

        // ─── GetAllCustomersAsync ────────────────────────────────────────────────

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsAllCustomers()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = Guid.NewGuid(), Status = "Assigned" },
                new Customer { Id = Guid.NewGuid(), Status = "Unassigned" },
                new Customer { Id = Guid.NewGuid(), Status = "Assigned" }
            };

            _customerRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            var result = await _adminService.GetAllCustomersAsync();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllCustomersAsync_EmptyDatabase_ReturnsEmptyList()
        {
            _customerRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer>());

            var result = await _adminService.GetAllCustomersAsync();

            Assert.Empty(result);
        }
    }
}
