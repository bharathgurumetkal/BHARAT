using System;
using System.Threading.Tasks;
using Insurance.Application.DTOs.Auth;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IPasswordHasher> _passwordHasherMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<ICustomerRepository> _customerRepositoryMock;
        private Mock<IAgentRepository> _agentRepositoryMock;
        private Mock<IClaimsOfficerRepository> _claimsOfficerRepositoryMock;
        private AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenServiceMock = new Mock<ITokenService>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _agentRepositoryMock = new Mock<IAgentRepository>();
            _claimsOfficerRepositoryMock = new Mock<IClaimsOfficerRepository>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object,
                _customerRepositoryMock.Object,
                _agentRepositoryMock.Object,
                _claimsOfficerRepositoryMock.Object
            );
        }

        // ─── RegisterAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_NewCustomer_CreatesUserAndCustomer()
        {
            var request = new RegisterRequestDto
            {
                Name = "John Doe", Email = "john@example.com",
                Password = "Password123", PhoneNumber = "1234567890", Role = "Customer"
            };

            _userRepositoryMock.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _passwordHasherMock.Setup(h => h.Hash(request.Password)).Returns("hashed_password");
            _tokenServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("test_token");

            var result = await _authService.RegisterAsync(request);

            Assert.NotNull(result);
            Assert.Equal("test_token", result.Token);
            Assert.Equal("Customer", result.Role);
            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == request.Email && u.Role == RoleType.Customer)), Times.Once);
            _customerRepositoryMock.Verify(r => r.AddAsync(It.Is<Customer>(c => c.Status == "Unassigned")), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_UserAlreadyExists_ThrowsException()
        {
            var request = new RegisterRequestDto { Email = "existing@example.com", Role = "Customer" };
            _userRepositoryMock.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(request));
            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_NonCustomerRole_ThrowsException()
        {
            // Only customers can self-register — agents and officers must be added by admin
            var request = new RegisterRequestDto
            {
                Name = "Agent User", Email = "agent@example.com",
                Password = "Password123", Role = "Agent"
            };

            _userRepositoryMock.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(request));
            Assert.Contains("admin", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_PasswordIsHashed_NotStoredAsPlaintext()
        {
            var request = new RegisterRequestDto
            {
                Name = "Jane", Email = "jane@example.com",
                Password = "MySecret", PhoneNumber = "9999999999", Role = "Customer"
            };

            _userRepositoryMock.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
            _passwordHasherMock.Setup(h => h.Hash(request.Password)).Returns("bcrypt_hash");
            _tokenServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("tok");

            await _authService.RegisterAsync(request);

            // Password should never be stored as plaintext
            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u =>
                u.PasswordHash == "bcrypt_hash" &&
                u.PasswordHash != request.Password
            )), Times.Once);
        }

        // ─── LoginAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            var request = new LoginRequestDto { Email = "john@example.com", Password = "Password123" };
            var user = new User { Id = Guid.NewGuid(), Email = request.Email, PasswordHash = "hashed_password", Role = RoleType.Customer };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(true);
            _tokenServiceMock.Setup(s => s.GenerateToken(user)).Returns("test_token");

            var result = await _authService.LoginAsync(request);

            Assert.NotNull(result);
            Assert.Equal("test_token", result.Token);
            Assert.Equal("Customer", result.Role);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsException()
        {
            var request = new LoginRequestDto { Email = "john@example.com", Password = "wrong_password" };
            var user = new User { Email = request.Email, PasswordHash = "hashed_password" };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(false);

            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(request));
            Assert.Contains("Invalid credentials", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsException()
        {
            var request = new LoginRequestDto { Email = "ghost@example.com", Password = "anything" };
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User)null);

            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_HardcodedAdminCredentials_ReturnsAdminToken()
        {
            // The system has a hardcoded admin bypass (admin@gmail.com / Admin@123)
            var request = new LoginRequestDto { Email = "admin@gmail.com", Password = "Admin@123" };
            _tokenServiceMock.Setup(s => s.GenerateToken(It.Is<User>(u => u.Role == RoleType.Admin))).Returns("admin_token");

            var result = await _authService.LoginAsync(request);

            Assert.Equal("admin_token", result.Token);
            Assert.Equal("Admin", result.Role);
            // Should NOT hit the user repository for hardcoded admin
            _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
