using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Insurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Insurance.Tests
{
    public class CustomerRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByAgentIdAsync_ReturnsDirectlyAssignedAndFromApplications()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new CustomerRepository(context);
            var agentId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var userId3 = Guid.NewGuid();

            // Customer 1: Directly assigned to Agent
            var customer1 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId1, 
                AssignedAgentId = agentId,
                User = new User { Id = userId1, Name = "C1", Email = "c1@t.com", PhoneNumber = "1", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };
            
            // Customer 2: Linked via PolicyApplication assigned to Agent
            var customer2 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId2, 
                AssignedAgentId = null,
                User = new User { Id = userId2, Name = "C2", Email = "c2@t.com", PhoneNumber = "2", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };
            
            // Application for Customer 2 assigned to Agent
            var productId = Guid.NewGuid();
            context.PolicyProducts.Add(new PolicyProduct { Id = productId, Name = "P1", PropertyCategory = "C1", Description = "D1" });
            context.PolicyApplications.Add(new PolicyApplication 
            { 
                Id = Guid.NewGuid(), ProductId = productId, CustomerId = userId2, 
                AssignedAgentId = agentId, PropertySubCategory = "S", Address = "A", RiskZone = "L" 
            });

            // Customer 3: Neither directly nor via application
            var customer3 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId3, 
                User = new User { Id = userId3, Name = "C3", Email = "c3@t.com", PhoneNumber = "3", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };

            context.Customers.AddRange(customer1, customer2, customer3);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByAgentIdAsync(agentId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.UserId == userId1);
            Assert.Contains(result, c => c.UserId == userId2);
            Assert.DoesNotContain(result, c => c.UserId == userId3);
        }
    }
}
