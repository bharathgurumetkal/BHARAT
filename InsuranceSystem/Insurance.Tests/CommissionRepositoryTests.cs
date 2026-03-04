using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Insurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Insurance.Tests
{
    public class CommissionRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ExistsForPolicyAsync_ReturnsTrueIfCommissionExists()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new CommissionRepository(context);
            var policyId = Guid.NewGuid();
            var agentId = Guid.NewGuid();

            context.Commissions.Add(new Commission { Id = Guid.NewGuid(), PolicyId = policyId, AgentId = agentId });
            await context.SaveChangesAsync();

            // Act
            var result = await repository.ExistsForPolicyAsync(policyId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetByAgentIdAsync_IncludesPolicyAndCustomerDetails()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new CommissionRepository(context);
            var agentId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var user = new User { Id = customerId, Name = "Test Customer", Email = "t@t.com", PhoneNumber = "123", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer };
            var customer = new Customer { Id = Guid.NewGuid(), UserId = customerId, User = user };
            var property = new Property { Id = Guid.NewGuid(), Address = "Addr", Category = "Cat", SubCategory = "Sub", RiskZone = "Low" };
            var policy = new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL1", Customer = customer, Property = property, CustomerId = customer.Id, PropertyId = property.Id };
            
            var commission = new Commission { Id = Guid.NewGuid(), AgentId = agentId, PolicyId = policy.Id, Policy = policy };

            context.Users.Add(user);
            context.Customers.Add(customer);
            context.Properties.Add(property);
            context.Policies.Add(policy);
            context.Commissions.Add(commission);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByAgentIdAsync(agentId);

            // Assert
            Assert.Single(result);
            Assert.NotNull(result[0].Policy);
            Assert.Equal("POL1", result[0].Policy.PolicyNumber);
            Assert.NotNull(result[0].Policy.Customer.User);
            Assert.Equal("Test Customer", result[0].Policy.Customer.User.Name);
        }
    }
}
