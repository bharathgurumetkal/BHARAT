using Insurance.Domain.Entities;
using Insurance.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Insurance.Tests
{
    public class SecurityTests
    {
        [Fact]
        public void PasswordHasher_HashesAndVerifiesCorrectly()
        {
            // Arrange
            var hasher = new PasswordHasher();
            var password = "SafePassword123";

            // Act
            var hash = hasher.Hash(password);
            var resultValid = hasher.Verify(password, hash);
            var resultInvalid = hasher.Verify("WrongPassword", hash);

            // Assert
            Assert.True(resultValid);
            Assert.False(resultInvalid);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void TokenService_GeneratesValidJwtToken()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKey12345678901234567890"); // Long enough for HMAC256
            configMock.Setup(c => c["Jwt:Issuer"]).Returns("InsuranceIssuer");
            configMock.Setup(c => c["Jwt:Audience"]).Returns("InsuranceAudience");

            var tokenService = new TokenService(configMock.Object);
            var user = new User 
            { 
                Id = Guid.NewGuid(), 
                Email = "test@test.com", 
                Role = Insurance.Domain.Enums.RoleType.Agent 
            };

            // Act
            var token = tokenService.GenerateToken(user);
            
            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.Equal("InsuranceIssuer", jwtToken.Issuer);
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Email && c.Value == "test@test.com");
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Agent");
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        }
    }
}
