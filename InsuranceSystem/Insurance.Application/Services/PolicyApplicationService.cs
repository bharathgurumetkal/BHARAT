using Insurance.Application.DTOs.PolicyApplication;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Application.DTOs.AuditLog;


namespace Insurance.Application.Services
{
    public class PolicyApplicationService : IPolicyApplicationService
    {
        private readonly IPolicyApplicationRepository _applicationRepository;
        private readonly IPolicyProductRepository _productRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public PolicyApplicationService(
            IPolicyApplicationRepository applicationRepository,
            IPolicyProductRepository productRepository,
            IPolicyRepository policyRepository,
            IPropertyRepository propertyRepository,
            ICustomerRepository customerRepository,
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            _applicationRepository = applicationRepository;
            _productRepository = productRepository;
            _policyRepository = policyRepository;
            _propertyRepository = propertyRepository;
            _customerRepository = customerRepository;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        public async Task<Guid> ApplyForProductAsync(Guid customerUserId, ApplyForProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null || !product.IsActive)
                throw new Exception("Product not found or not active.");

            if (dto.RequestedCoverageAmount > product.MaxCoverageAmount)
                throw new Exception($"Requested coverage exceeds max allowed: {product.MaxCoverageAmount}.");

            // Calculate premium based on product base rate
            decimal premium = dto.RequestedCoverageAmount * (product.BaseRatePercentage / 100m);
            if (dto.RiskZone == "High")
                premium += dto.RequestedCoverageAmount * 0.01m;
            if (!dto.HasSecuritySystem)
                premium += 500m;

            var application = new PolicyApplication
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                CustomerId = customerUserId,
                PropertySubCategory = dto.PropertySubCategory,
                Address = dto.Address,
                YearBuilt = dto.YearBuilt,
                MarketValue = dto.MarketValue,
                RiskZone = dto.RiskZone,
                HasSecuritySystem = dto.HasSecuritySystem,
                RequestedCoverageAmount = dto.RequestedCoverageAmount,
                CalculatedPremium = premium,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                customerUserId,
                "Application Submitted",
                $"Your application for {product.Name} has been received.",
                "Info"
            );

            await _auditLogService.LogAsync(new AuditLogEntry
            {
                Action = AuditAction.ApplicationSubmitted.ToString(),
                EntityType = "PolicyApplication",
                EntityId = application.Id.ToString(),
                Description = $"Application submitted for product {product.Name}."
            });

            return application.Id;
        }

        public async Task AssignAgentAsync(Guid applicationId, Guid agentUserId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
                throw new Exception("Application not found.");

            if (application.Status != "Submitted")
                throw new Exception($"Cannot assign agent. Current status: {application.Status}. Expected: Submitted.");

            application.AssignedAgentId = agentUserId;
            application.Status = "AssignedToAgent";
            application.AssignedAt = DateTime.UtcNow;

            // Also assign the customer to this agent in the Customers table
            var customer = await _customerRepository.GetByUserIdAsync(application.CustomerId);
            if (customer != null && customer.AssignedAgentId == null)
            {
                customer.AssignedAgentId = agentUserId;
                customer.Status = "Assigned";
            }

            await _applicationRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                agentUserId,
                "New Application Assigned",
                $"A new policy application for {application.Product?.Name ?? "Insurance"} has been assigned to you.",
                "Info"
            );

            await _auditLogService.LogAsync(new AuditLogEntry
            {
                Action = AuditAction.ApplicationAssigned.ToString(),
                EntityType = "PolicyApplication",
                EntityId = application.Id.ToString(),
                Description = $"Application assigned to agent {agentUserId}."
            });
        }

        public async Task ApproveApplicationAsync(Guid applicationId, Guid agentUserId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
                throw new Exception("Application not found.");

            if (application.AssignedAgentId != agentUserId)
            {
                var cust = await _customerRepository.GetByUserIdAsync(application.CustomerId);
                if (cust?.AssignedAgentId == agentUserId && application.Status == "Submitted")
                {
                    application.AssignedAgentId = agentUserId;
                    application.AssignedAt = DateTime.UtcNow;
                    application.Status = "AssignedToAgent"; // Explicitly transition before approving
                }
                else
                {
                    var currentAgent = application.AssignedAgent?.Name ?? "another representative";
                    throw new Exception($"Assignment Mismatch: This application is currently assigned to {currentAgent}. You cannot approve it.");
                }
            }

            if (application.Status != "AssignedToAgent" && application.Status != "Submitted")
                throw new Exception($"Invalid Status: Cannot approve application in '{application.Status}' state. Expected: Submitted or AssignedToAgent.");

            application.Status = "ApprovedByAgent";
            application.ReviewedAt = DateTime.UtcNow;

            var customer = await _customerRepository.GetByUserIdAsync(application.CustomerId);
            if (customer == null)
                throw new Exception("Customer record not found for this user.");

            // Create a matching Property
            var property = new Property
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Category = application.Product?.PropertyCategory ?? "General",
                SubCategory = application.PropertySubCategory,
                Address = application.Address,
                YearBuilt = application.YearBuilt,
                MarketValue = application.MarketValue,
                RiskZone = application.RiskZone,
                HasSecuritySystem = application.HasSecuritySystem
            };

            await _propertyRepository.AddAsync(property);

            // Create a Draft Policy
            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = $"POL-{DateTime.UtcNow.Ticks}",
                CustomerId = customer.Id,
                PropertyId = property.Id,
                ApplicationId = application.Id,
                CoverageAmount = application.RequestedCoverageAmount,
                Premium = application.CalculatedPremium,
                Status = PolicyStatus.Draft,
                CreatedByAdminId = agentUserId  // Agent is a valid User and satisfies this foreign key constraint
            };

            await _policyRepository.AddPolicyAsync(policy);
            await _applicationRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                application.CustomerId,
                "Application Approved",
                $"Congratulations! Your application for {application.Product?.Name ?? "Insurance"} has been approved. Please pay the premium to activate your policy.",
                "Success"
            );

            await _auditLogService.LogAsync(new AuditLogEntry
            {
                Action = AuditAction.ApplicationApproved.ToString(),
                EntityType = "PolicyApplication",
                EntityId = application.Id.ToString(),
                Description = "Application approved and draft policy created."
            });
        }

        public async Task RejectApplicationAsync(Guid applicationId, Guid agentUserId)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
                throw new Exception("Application not found.");

            if (application.AssignedAgentId != agentUserId)
            {
                var cust = await _customerRepository.GetByUserIdAsync(application.CustomerId);
                if (cust?.AssignedAgentId == agentUserId && application.Status == "Submitted")
                {
                    application.AssignedAgentId = agentUserId;
                    application.AssignedAt = DateTime.UtcNow;
                    application.Status = "AssignedToAgent";
                }
                else
                {
                    throw new Exception("Assignment Mismatch: You are not assigned to this application.");
                }
            }

            if (application.Status != "AssignedToAgent" && application.Status != "Submitted")
                throw new Exception($"Invalid Status: Cannot reject application in '{application.Status}' state. Expected: Submitted or AssignedToAgent.");

            application.Status = "RejectedByAgent";
            application.ReviewedAt = DateTime.UtcNow;

            await _applicationRepository.SaveChangesAsync();

            await _notificationService.CreateAsync(
                application.CustomerId,
                "Application Rejected",
                $"We regret to inform you that your application for {application.Product?.Name ?? "Insurance"} has been rejected.",
                "Risk"
            );

            await _auditLogService.LogAsync(new AuditLogEntry
            {
                Action = AuditAction.ApplicationRejected.ToString(),
                EntityType = "PolicyApplication",
                EntityId = application.Id.ToString(),
                Description = "Application rejected by agent."
            });
        }

        public async Task<List<PolicyApplicationDto>> GetAssignedApplicationsAsync(Guid agentUserId)
        {
            var applications = await _applicationRepository.GetByAgentIdAsync(agentUserId);
            return applications.Select(a => new PolicyApplicationDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                ProductName = a.Product?.Name ?? "",
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.Name ?? "",
                AssignedAgentId = a.AssignedAgentId,
                AssignedAgentName = a.AssignedAgent?.Name,
                PropertySubCategory = a.PropertySubCategory,
                Address = a.Address,
                YearBuilt = a.YearBuilt,
                MarketValue = a.MarketValue,
                RiskZone = a.RiskZone,
                HasSecuritySystem = a.HasSecuritySystem,
                RequestedCoverageAmount = a.RequestedCoverageAmount,
                CalculatedPremium = a.CalculatedPremium,
                Status = a.Status,
                SubmittedAt = a.SubmittedAt,
                AssignedAt = a.AssignedAt,
                ReviewedAt = a.ReviewedAt
            }).ToList();
        }

        public async Task<List<PolicyApplicationDto>> GetApplicationsByCustomerAsync(Guid customerUserId)
        {
            var applications = await _applicationRepository.GetByCustomerIdAsync(customerUserId);
            return applications.Select(a => new PolicyApplicationDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                ProductName = a.Product?.Name ?? "",
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.Name ?? "",
                AssignedAgentId = a.AssignedAgentId,
                AssignedAgentName = a.AssignedAgent?.Name,
                PropertySubCategory = a.PropertySubCategory,
                Address = a.Address,
                YearBuilt = a.YearBuilt,
                MarketValue = a.MarketValue,
                RiskZone = a.RiskZone,
                HasSecuritySystem = a.HasSecuritySystem,
                RequestedCoverageAmount = a.RequestedCoverageAmount,
                CalculatedPremium = a.CalculatedPremium,
                Status = a.Status,
                SubmittedAt = a.SubmittedAt,
                AssignedAt = a.AssignedAt,
                ReviewedAt = a.ReviewedAt
            }).ToList();
        }

        public async Task<List<PolicyApplicationDto>> GetAllApplicationsAsync()
        {
            var applications = await _applicationRepository.GetAllAsync();
            return applications.Select(a => new PolicyApplicationDto
            {
                Id = a.Id,
                ProductId = a.ProductId,
                ProductName = a.Product?.Name ?? "",
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.Name ?? "",
                AssignedAgentId = a.AssignedAgentId,
                AssignedAgentName = a.AssignedAgent?.Name,
                PropertySubCategory = a.PropertySubCategory,
                Address = a.Address,
                YearBuilt = a.YearBuilt,
                MarketValue = a.MarketValue,
                RiskZone = a.RiskZone,
                HasSecuritySystem = a.HasSecuritySystem,
                RequestedCoverageAmount = a.RequestedCoverageAmount,
                CalculatedPremium = a.CalculatedPremium,
                Status = a.Status,
                SubmittedAt = a.SubmittedAt,
                AssignedAt = a.AssignedAt,
                ReviewedAt = a.ReviewedAt
            }).ToList();
        }
    }
}
