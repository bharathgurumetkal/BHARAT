using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Insurance.Application.DTOs.Claim;
using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Moq;
using Xunit;

namespace Insurance.Tests
{
    public class ClaimServiceTests
    {
        private Mock<IClaimRepository> _claimRepositoryMock;
        private Mock<IPolicyRepository> _policyRepositoryMock;
        private Mock<IAiClaimClient> _aiClaimClientMock;
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private ClaimService _claimService;

        public ClaimServiceTests()
        {
            _claimRepositoryMock = new Mock<IClaimRepository>();
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _aiClaimClientMock = new Mock<IAiClaimClient>();
            _notificationServiceMock = new Mock<INotificationService>();
            _userRepositoryMock = new Mock<IUserRepository>();

            _claimService = new ClaimService(
                _claimRepositoryMock.Object,
                _policyRepositoryMock.Object,
                _aiClaimClientMock.Object,
                _notificationServiceMock.Object,
                _userRepositoryMock.Object
            );
        }

        // ─── SettleClaimAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task SettleClaimAsync_ValidApprovedClaim_UpdatesStatusToSettled()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.Approved };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await _claimService.SettleClaimAsync(claimId);

            Assert.Equal(ClaimStatus.Settled, claim.Status);
            _claimRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _notificationServiceMock.Verify(n => n.CreateAsync(It.IsAny<Guid>(), "Claim Settled", It.IsAny<string>(), "Success"), Times.Once);
        }

        [Fact]
        public async Task SettleClaimAsync_ClaimNotFound_ThrowsException()
        {
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Claim)null);

            await Assert.ThrowsAsync<Exception>(() => _claimService.SettleClaimAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task SettleClaimAsync_ClaimNotApproved_ThrowsWithCorrectMessage()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.Submitted };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            var ex = await Assert.ThrowsAsync<Exception>(() => _claimService.SettleClaimAsync(claimId));
            Assert.Equal("Only approved claims can be settled.", ex.Message);
        }

        [Fact]
        public async Task SettleClaimAsync_UnderReviewClaim_ThrowsException()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.UnderReview };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await Assert.ThrowsAsync<Exception>(() => _claimService.SettleClaimAsync(claimId));
        }

        // ─── StartReviewAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task StartReviewAsync_ValidSubmittedClaim_UpdatesStatusToUnderReview()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.Submitted };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await _claimService.StartReviewAsync(claimId);

            Assert.Equal(ClaimStatus.UnderReview, claim.Status);
            _claimRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task StartReviewAsync_ClaimNotFound_ThrowsException()
        {
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Claim)null);

            await Assert.ThrowsAsync<Exception>(() => _claimService.StartReviewAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task StartReviewAsync_AlreadyUnderReview_ThrowsException()
        {
            // Cannot start review if claim is not in Submitted status
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.UnderReview };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await Assert.ThrowsAsync<Exception>(() => _claimService.StartReviewAsync(claimId));
        }

        // ─── ReviewClaimAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task ReviewClaimAsync_ApproveAction_UpdatesStatusToApproved()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.UnderReview, ClaimAmount = 1000 };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await _claimService.ReviewClaimAsync(claimId, true);

            Assert.Equal(ClaimStatus.Approved, claim.Status);
            _claimRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _notificationServiceMock.Verify(n => n.CreateAsync(It.IsAny<Guid>(), "Claim Approved", It.IsAny<string>(), "Success"), Times.Once);
        }

        [Fact]
        public async Task ReviewClaimAsync_RejectAction_UpdatesStatusToRejected()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.UnderReview, ClaimAmount = 5000 };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await _claimService.ReviewClaimAsync(claimId, false);

            Assert.Equal(ClaimStatus.Rejected, claim.Status);
            _notificationServiceMock.Verify(n => n.CreateAsync(It.IsAny<Guid>(), "Claim Rejected", It.IsAny<string>(), "Risk"), Times.Once);
        }

        [Fact]
        public async Task ReviewClaimAsync_ClaimNotFound_ThrowsException()
        {
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Claim)null);

            await Assert.ThrowsAsync<Exception>(() => _claimService.ReviewClaimAsync(Guid.NewGuid(), true));
        }

        [Fact]
        public async Task ReviewClaimAsync_ClaimNotUnderReview_ThrowsException()
        {
            var claimId = Guid.NewGuid();
            var claim = new Claim { Id = claimId, Status = ClaimStatus.Submitted };
            _claimRepositoryMock.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            await Assert.ThrowsAsync<Exception>(() => _claimService.ReviewClaimAsync(claimId, true));
        }

        // ─── SubmitClaimAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitClaimAsync_NormalAmount_CreatesSubmittedClaim()
        {
            // Amount <= 80% of coverage → stays Submitted
            var policyId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, Status = PolicyStatus.Active, CoverageAmount = 10000 };
            var dto = new SubmitClaimDto
            {
                PolicyId = policyId, ClaimAmount = 5000,
                Reason = "Minor flood", Documents = new List<string> { "doc1_url" }
            };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("ClaimsOfficer")).ReturnsAsync(new List<User>());
            _aiClaimClientMock.Setup(c => c.AnalyzeClaimAsync(It.IsAny<AiClaimRequestDto>()))
                .ReturnsAsync(new AiClaimResponseDto { RiskScore = 20, RiskLevel = "Low" });

            await _claimService.SubmitClaimAsync(dto);

            _claimRepositoryMock.Verify(r => r.AddAsync(It.Is<Claim>(c => c.Status == ClaimStatus.Submitted)), Times.Once);
        }

        [Fact]
        public async Task SubmitClaimAsync_HighAmount_FlagsAsUnderReview()
        {
            // Amount > 80% of coverage → auto-flagged as UnderReview
            var policyId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, Status = PolicyStatus.Active, CoverageAmount = 10000 };
            var dto = new SubmitClaimDto
            {
                PolicyId = policyId, ClaimAmount = 9000,  // 90% > 80%
                Reason = "Major loss", Documents = new List<string> { "url1" }
            };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("ClaimsOfficer")).ReturnsAsync(new List<User>());
            _aiClaimClientMock.Setup(c => c.AnalyzeClaimAsync(It.IsAny<AiClaimRequestDto>()))
                .ReturnsAsync(new AiClaimResponseDto { RiskScore = 10, RiskLevel = "Low" });

            await _claimService.SubmitClaimAsync(dto);

            _claimRepositoryMock.Verify(r => r.AddAsync(It.Is<Claim>(c => c.Status == ClaimStatus.UnderReview)), Times.Once);
        }

        [Fact]
        public async Task SubmitClaimAsync_InactivePolicy_ThrowsException()
        {
            var policyId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, Status = PolicyStatus.Draft };
            var dto = new SubmitClaimDto { PolicyId = policyId, ClaimAmount = 1000, Documents = new List<string>() };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);

            var ex = await Assert.ThrowsAsync<Exception>(() => _claimService.SubmitClaimAsync(dto));
            Assert.Contains("not active", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SubmitClaimAsync_PolicyNotFound_ThrowsException()
        {
            _policyRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Policy)null);
            var dto = new SubmitClaimDto { PolicyId = Guid.NewGuid(), Documents = new List<string>() };

            await Assert.ThrowsAsync<Exception>(() => _claimService.SubmitClaimAsync(dto));
        }

        [Fact]
        public async Task SubmitClaimAsync_NotifiesClaimsOfficers()
        {
            var policyId = Guid.NewGuid();
            var officerId = Guid.NewGuid();
            var policy = new Policy { Id = policyId, Status = PolicyStatus.Active, CoverageAmount = 50000 };
            var officers = new List<User> { new User { Id = officerId, Name = "Officer A" } };
            var dto = new SubmitClaimDto
            {
                PolicyId = policyId, ClaimAmount = 1000,
                Reason = "Theft", Documents = new List<string>()
            };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("ClaimsOfficer")).ReturnsAsync(officers);
            _aiClaimClientMock.Setup(c => c.AnalyzeClaimAsync(It.IsAny<AiClaimRequestDto>()))
                .ReturnsAsync(new AiClaimResponseDto { RiskScore = 5, RiskLevel = "Low" });

            await _claimService.SubmitClaimAsync(dto);

            _notificationServiceMock.Verify(
                n => n.CreateAsync(officerId, "New Claim for Review", It.IsAny<string>(), "Info"),
                Times.Once
            );
        }

        // ─── SubmitClaimAsync – AI High-Risk Alert Path ───────────────────────────

        [Fact]
        public async Task SubmitClaimAsync_AiScoreHighRisk_SendsFraudAlertToAdmins()
        {
            // AI score >= 70 should trigger a "Fraud Risk Alert" notification to all Admins
            var policyId = Guid.NewGuid();
            var adminId  = Guid.NewGuid();
            var policy   = new Policy { Id = policyId, Status = PolicyStatus.Active, CoverageAmount = 10000 };
            var admins   = new List<User> { new User { Id = adminId, Name = "Admin A" } };
            var dto = new SubmitClaimDto
            {
                PolicyId = policyId, ClaimAmount = 1000,
                Reason = "Suspicious", Documents = new List<string>()
            };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("ClaimsOfficer")).ReturnsAsync(new List<User>());
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("Admin")).ReturnsAsync(admins);
            _aiClaimClientMock.Setup(c => c.AnalyzeClaimAsync(It.IsAny<AiClaimRequestDto>()))
                .ReturnsAsync(new AiClaimResponseDto { RiskScore = 85, RiskLevel = "High", FraudProbability = 0.9 });

            await _claimService.SubmitClaimAsync(dto);

            _notificationServiceMock.Verify(
                n => n.CreateAsync(adminId, "🚨 Fraud Risk Alert", It.IsAny<string>(), "Risk"),
                Times.Once
            );
        }

        [Fact]
        public async Task SubmitClaimAsync_AiScoreHighRisk_SendsUrgentWarningToClaimsOfficers()
        {
            // AI score >= 70 should also trigger a "High Risk Claim Detected" Warning to Claims Officers
            var policyId = Guid.NewGuid();
            var officerId = Guid.NewGuid();
            var policy   = new Policy { Id = policyId, Status = PolicyStatus.Active, CoverageAmount = 10000 };
            var officers = new List<User> { new User { Id = officerId, Name = "Officer B" } };
            var dto = new SubmitClaimDto
            {
                PolicyId = policyId, ClaimAmount = 2000,
                Reason = "Fire damage", Documents = new List<string>()
            };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            // First call (initial notification) returns this officer, second call (high-risk loop) returns same
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("ClaimsOfficer")).ReturnsAsync(officers);
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("Admin")).ReturnsAsync(new List<User>());
            _aiClaimClientMock.Setup(c => c.AnalyzeClaimAsync(It.IsAny<AiClaimRequestDto>()))
                .ReturnsAsync(new AiClaimResponseDto { RiskScore = 75, RiskLevel = "High", FraudProbability = 0.8 });

            await _claimService.SubmitClaimAsync(dto);

            _notificationServiceMock.Verify(
                n => n.CreateAsync(officerId, "High Risk Claim Detected", It.IsAny<string>(), "Warning"),
                Times.Once
            );
        }

        [Fact]
        public async Task SubmitClaimAsync_AiScoreBelowThreshold_DoesNotSendFraudAlert()
        {
            // AI score < 70 → NO fraud alert should be sent to Admins
            var policyId = Guid.NewGuid();
            var adminId  = Guid.NewGuid();
            var policy   = new Policy { Id = policyId, Status = PolicyStatus.Active, CoverageAmount = 10000 };
            var dto = new SubmitClaimDto
            {
                PolicyId = policyId, ClaimAmount = 500,
                Reason = "Minor damage", Documents = new List<string>()
            };

            _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId)).ReturnsAsync(policy);
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("ClaimsOfficer")).ReturnsAsync(new List<User>());
            _userRepositoryMock.Setup(r => r.GetByRoleAsync("Admin")).ReturnsAsync(new List<User> { new User { Id = adminId } });
            _aiClaimClientMock.Setup(c => c.AnalyzeClaimAsync(It.IsAny<AiClaimRequestDto>()))
                .ReturnsAsync(new AiClaimResponseDto { RiskScore = 30, RiskLevel = "Low", FraudProbability = 0.1 });

            await _claimService.SubmitClaimAsync(dto);

            _notificationServiceMock.Verify(
                n => n.CreateAsync(adminId, "🚨 Fraud Risk Alert", It.IsAny<string>(), "Risk"),
                Times.Never
            );
        }

        // ─── GetClaimsOfficerDashboardSummaryAsync ────────────────────────────────

        [Fact]
        public async Task GetClaimsOfficerDashboardSummaryAsync_ReturnsCorrectCounts()
        {
            var officerId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Submitted,  ClaimAmount = 100, CreatedAt = DateTime.UtcNow.AddDays(-1), Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.UnderReview, ClaimAmount = 200, CreatedAt = DateTime.UtcNow.AddDays(-2), Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Approved,   ClaimAmount = 300, CreatedAt = DateTime.UtcNow.AddDays(-3), Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Rejected,   ClaimAmount = 400, CreatedAt = DateTime.UtcNow.AddDays(-4), Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Settled,    ClaimAmount = 500, CreatedAt = DateTime.UtcNow.AddDays(-5), Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Submitted,  ClaimAmount = 600, CreatedAt = DateTime.UtcNow.AddDays(-6), Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
            };
            _claimRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(claims);

            var result = await _claimService.GetClaimsOfficerDashboardSummaryAsync(officerId);

            Assert.Equal(6, result.Total);
            Assert.Equal(2, result.Submitted);
            Assert.Equal(1, result.UnderReview);
            Assert.Equal(1, result.Approved);
            Assert.Equal(1, result.Rejected);
            Assert.Equal(1, result.Settled);
        }

        [Fact]
        public async Task GetClaimsOfficerDashboardSummaryAsync_ReturnsAtMostFiveRecentClaims()
        {
            // Given 6 claims, RecentClaims should only contain the 5 most recent
            var officerId = Guid.NewGuid();
            var claims = new List<Claim>();
            for (int i = 1; i <= 6; i++)
            {
                claims.Add(new Claim
                {
                    Id = Guid.NewGuid(),
                    AssignedOfficerId = officerId,
                    Status = ClaimStatus.Submitted,
                    ClaimAmount = i * 100,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() }
                });
            }
            _claimRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(claims);

            var result = await _claimService.GetClaimsOfficerDashboardSummaryAsync(officerId);

            Assert.Equal(5, result.RecentClaims.Count);
        }

        [Fact]
        public async Task GetClaimsOfficerDashboardSummaryAsync_RecentClaimsAreOrderedMostRecentFirst()
        {
            var officerId = Guid.NewGuid();
            var oldestDate = DateTime.UtcNow.AddDays(-10);
            var newestDate = DateTime.UtcNow.AddDays(-1);
            var claims = new List<Claim>
            {
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Submitted, CreatedAt = oldestDate, Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
                new Claim { Id = Guid.NewGuid(), AssignedOfficerId = officerId, Status = ClaimStatus.Approved,  CreatedAt = newestDate, Policy = new Policy { Customer = new Customer { User = new User() }, Property = new Property() } },
            };
            _claimRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(claims);

            var result = await _claimService.GetClaimsOfficerDashboardSummaryAsync(officerId);

            // First item in RecentClaims should be the newest one
            Assert.Equal(newestDate, result.RecentClaims.First().CreatedAt);
        }

        [Fact]
        public async Task GetClaimsOfficerDashboardSummaryAsync_NoClaims_ReturnsZeroCountsAndEmptyList()
        {
            var officerId = Guid.NewGuid();
            _claimRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Claim>());

            var result = await _claimService.GetClaimsOfficerDashboardSummaryAsync(officerId);

            Assert.Equal(0, result.Total);
            Assert.Equal(0, result.Submitted);
            Assert.Empty(result.RecentClaims);
        }
    }
}
