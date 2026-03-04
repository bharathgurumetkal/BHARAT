using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Insurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Insurance.Tests
{
    public class PolicyRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetPoliciesByAgentAsync_ReturnsByAssignmentAndCreation()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new PolicyRepository(context);
            var agentId = Guid.NewGuid();
            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();

            // Customer 1 assigned to Agent
            var customer1 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = customerId1, 
                AssignedAgentId = agentId,
                User = new User { Id = customerId1, Name = "C1", Email = "c1@t", PhoneNumber = "1", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };
            
            // Customer 2 NOT assigned to Agent
            var customer2 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = customerId2, 
                User = new User { Id = customerId2, Name = "C2", Email = "c2@t", PhoneNumber = "2", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };

            context.Customers.AddRange(customer1, customer2);

            // Policy 1: Linked to Customer 1 (Assigned)
            var p1 = new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL1", CustomerId = customer1.Id, CreatedByAdminId = Guid.NewGuid() };
            
            // Policy 2: Linked to Customer 2 (Unassigned) but Created by Agent
            var p2 = new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL2", CustomerId = customer2.Id, CreatedByAdminId = agentId };
            
            // Policy 3: Unassigned and Created by someone else
            var p3 = new Policy { Id = Guid.NewGuid(), PolicyNumber = "POL3", CustomerId = customer2.Id, CreatedByAdminId = Guid.NewGuid() };

            context.Policies.AddRange(p1, p2, p3);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetPoliciesByAgentAsync(agentId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == p1.Id);
            Assert.Contains(result, p => p.Id == p2.Id);
            Assert.DoesNotContain(result, p => p.Id == p3.Id);
        }
    }
}
