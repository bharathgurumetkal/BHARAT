using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Insurance.Tests
{
    public class ClaimRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsClaimToDatabase()
        {
            // Arrange
            var context = GetDbContext();
            var repository = new ClaimRepository(context);
            var claim = new Claim { Id = Guid.NewGuid(), Reason = "Water Damage" };

            // Act
            await repository.AddAsync(claim);
            await repository.SaveChangesAsync();

            // Assert
            var savedClaim = await context.Claims.FindAsync(claim.Id);
            Assert.NotNull(savedClaim);
            Assert.Equal("Water Damage", savedClaim.Reason);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectClaim()
        {
            // Arrange
            var context = GetDbContext();
            var claimId = Guid.NewGuid();
            context.Claims.Add(new Claim { Id = claimId, Reason = "Fire Damage" });
            context.SaveChanges();

            var repository = new ClaimRepository(context);

            // Act
            var result = await repository.GetByIdAsync(claimId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(claimId, result.Id);
            Assert.Equal("Fire Damage", result.Reason);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsClaimsInPriorityOrder()
        {
            // Arrange
            var context = GetDbContext();
            var user = new User { Id = Guid.NewGuid(), Name = "Test User", Email = "test@test.com", PhoneNumber = "1234567890", PasswordHash = "...", Role = Insurance.Domain.Enums.RoleType.Customer };
            context.Users.Add(user);

            var customer = new Customer { Id = Guid.NewGuid(), UserId = user.Id, User = user };
            context.Customers.Add(customer);

            var property = new Property { Id = Guid.NewGuid(), Address = "123 Main St", Category = "Home", SubCategory = "Single Family", RiskZone = "Low" };
            context.Properties.Add(property);

            var policy = new Policy { 
                Id = Guid.NewGuid(), 
                PolicyNumber = "P001",
                CustomerId = customer.Id,
                Customer = customer,
                PropertyId = property.Id,
                Property = property
            };
            context.Policies.Add(policy);

            var claim1 = new Claim { Id = Guid.NewGuid(), PolicyId = policy.Id, Policy = policy, Reason = "Test1", AiRiskScore = 50, CreatedAt = DateTime.UtcNow };
            var claim2 = new Claim { Id = Guid.NewGuid(), PolicyId = policy.Id, Policy = policy, Reason = "Test2", AiRiskScore = 90, CreatedAt = DateTime.UtcNow };
            var claim3 = new Claim { Id = Guid.NewGuid(), PolicyId = policy.Id, Policy = policy, Reason = "Test3", AiRiskScore = null, CreatedAt = DateTime.UtcNow };

            context.Claims.AddRange(claim1, claim2, claim3);
            context.SaveChanges();

            var repository = new ClaimRepository(context);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(claim2.Id, result[0].Id); // High Risk (90) first
            Assert.Equal(claim1.Id, result[1].Id); // Medium Risk (50) second
            Assert.Equal(claim3.Id, result[2].Id); // Unscored (null) last
        }
    }
}
