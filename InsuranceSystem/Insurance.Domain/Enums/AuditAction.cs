namespace Insurance.Domain.Enums;

public enum AuditAction
{
    UserLogin,
    UserLogout,
    ProductCreated,
    ProductUpdated,
    ApplicationSubmitted,
    ApplicationAssigned,
    ApplicationApproved,
    ApplicationRejected,
    PolicyActivated,
    PolicyRenewed,
    PolicyCancelled,
    ClaimSubmitted,
    ClaimUnderReview,
    ClaimApproved,
    ClaimRejected,
    ClaimSettled,
    PaymentCompleted,
    CommissionGenerated
}
