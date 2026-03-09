namespace Insurance.Application.DTOs.Claim
{
    public class ClaimsOfficerDashboardSummaryDto
    {
        public int Total { get; set; }
        public int Submitted { get; set; }
        public int UnderReview { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Settled { get; set; }

        public List<ClaimDto> RecentClaims { get; set; } = new();
    }
}
