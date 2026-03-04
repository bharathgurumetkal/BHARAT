using Insurance.Application.DTOs.Auth;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly IClaimsOfficerRepository _claimsOfficerRepository;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ICustomerRepository customerRepository,
        IAgentRepository agentRepository,
        IClaimsOfficerRepository claimsOfficerRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _customerRepository = customerRepository;
        _agentRepository = agentRepository;
        _claimsOfficerRepository = claimsOfficerRepository;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new Exception("User already exists.");

        var role = Enum.Parse<RoleType>(request.Role, true);

        // Only customers can self-register. Agents and ClaimsOfficer must be created by Admin.
        if (role != RoleType.Customer)
            throw new Exception("Only customers can register. Agents and Claims Officers must be added by an admin.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = role
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Create profile based on role
        switch (role)
        {
            case RoleType.Customer:
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Status = "Unassigned"
                };

                await _customerRepository.AddAsync(customer);
                break;

            case RoleType.Agent:
                var agent = new Agent
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LicenseNumber = "LIC-" + DateTime.UtcNow.Ticks,
                    JoinedDate = DateTime.UtcNow
                };

                await _agentRepository.AddAsync(agent);
                break;

            case RoleType.ClaimsOfficer:
                var officer = new ClaimsOfficer
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Department = "Property Claims"
                };

                await _claimsOfficerRepository.AddAsync(officer);
                break;

            case RoleType.Admin:
                // No additional profile required for now
                break;
        }

        await _customerRepository.SaveChangesAsync(); // one SaveChanges is enough if same DbContext

        var token = _tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Hardcoded admin credentials
        if (request.Email == "admin@gmail.com" && request.Password == "Admin@123")
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Email = "admin",
                Role = RoleType.Admin
            };

            var adminToken = _tokenService.GenerateToken(adminUser);

            return new AuthResponseDto
            {
                Token = adminToken,
                Email = adminUser.Email,
                Role = adminUser.Role.ToString()
            };
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var token = _tokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}