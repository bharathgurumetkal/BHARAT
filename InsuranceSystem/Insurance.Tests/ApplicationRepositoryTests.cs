using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Insurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Insurance.Tests
{
    public class ApplicationRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetByAgentIdAsync_ReturnsDirectlyAssignedAndSubmittedForAssignedCustomers()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new PolicyApplicationRepository(context);
            var agentId = Guid.NewGuid();
            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();

            // Customer 1 assigned to Agent
            var customer1 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = customerId1, 
                AssignedAgentId = agentId,
                User = new User { Id = customerId1, Name = "C1", Email = "c1@test.com", PhoneNumber = "1", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };
            
            // Customer 2 NOT assigned to Agent
            var customer2 = new Customer 
            { 
                Id = Guid.NewGuid(), 
                UserId = customerId2, 
                User = new User { Id = customerId2, Name = "C2", Email = "c2@test.com", PhoneNumber = "2", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer }
            };

            context.Customers.AddRange(customer1, customer2);

            var productId = Guid.NewGuid();
            context.PolicyProducts.Add(new PolicyProduct { Id = productId, Name = "P1", PropertyCategory = "C1", Description = "D1" });

            // App 1: Directly assigned to Agent
            var app1 = new PolicyApplication 
            { 
                Id = Guid.NewGuid(), ProductId = productId, CustomerId = customerId1, 
                AssignedAgentId = agentId, Status = "UnderReview", SubmittedAt = DateTime.UtcNow.AddHours(-2),
                PropertySubCategory = "S1", Address = "A1", RiskZone = "L1"
            };
            
            // App 2: Unassigned but Customer belongs to Agent
            var app2 = new PolicyApplication 
            { 
                Id = Guid.NewGuid(), ProductId = productId, CustomerId = customerId1, 
                AssignedAgentId = null, Status = "Submitted", SubmittedAt = DateTime.UtcNow.AddHours(-1),
                PropertySubCategory = "S2", Address = "A2", RiskZone = "L2"
            };
            
            // App 3: Unassigned and Customer NOT belonging to Agent
            var app3 = new PolicyApplication 
            { 
                Id = Guid.NewGuid(), ProductId = productId, CustomerId = customerId2, 
                AssignedAgentId = null, Status = "Submitted", SubmittedAt = DateTime.UtcNow,
                PropertySubCategory = "S3", Address = "A3", RiskZone = "L3"
            };

            context.PolicyApplications.AddRange(app1, app2, app3);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByAgentIdAsync(agentId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Id == app1.Id);
            Assert.Contains(result, a => a.Id == app2.Id);
            Assert.DoesNotContain(result, a => a.Id == app3.Id);
        }
    }
}
