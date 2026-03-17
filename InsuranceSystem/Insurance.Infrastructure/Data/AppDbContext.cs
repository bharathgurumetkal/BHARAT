using Microsoft.EntityFrameworkCore;
using Insurance.Domain.Entities;

namespace Insurance.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<ClaimsOfficer> ClaimsOfficers => Set<ClaimsOfficer>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<PolicyProduct> PolicyProducts => Set<PolicyProduct>();
    public DbSet<PolicyApplication> PolicyApplications => Set<PolicyApplication>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Customer → User (Owner)
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Customer → Assigned Agent
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.AssignedAgent)
            .WithMany()
            .HasForeignKey(c => c.AssignedAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Policy → Customer
        modelBuilder.Entity<Policy>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Policy>()
    .HasOne<User>()   // Admin is stored in Users table
    .WithMany()
    .HasForeignKey(p => p.CreatedByAdminId)
    .OnDelete(DeleteBehavior.Restrict);

        // Policy → Property
        modelBuilder.Entity<Policy>()
            .HasOne(p => p.Property)
            .WithMany()
            .HasForeignKey(p => p.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Claim → Policy
        modelBuilder.Entity<Claim>()
            .HasOne(c => c.Policy)
            .WithMany()
            .HasForeignKey(c => c.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment → Policy
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Policy)
            .WithMany()
            .HasForeignKey(p => p.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Agent>()
    .HasOne(a => a.User)
    .WithMany()
    .HasForeignKey(a => a.UserId)
    .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClaimsOfficer>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClaimDocument>()
    .HasOne(d => d.Claim)
    .WithMany(c => c.Documents)
    .HasForeignKey(d => d.ClaimId);

        modelBuilder.Entity<Policy>()
    .Property(p => p.CoverageAmount)
    .HasPrecision(18, 2);

        modelBuilder.Entity<Policy>()
            .Property(p => p.Premium)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Property>()
            .Property(p => p.MarketValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Claim>()
            .Property(c => c.ClaimAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // PolicyProduct precision
        modelBuilder.Entity<PolicyProduct>()
            .Property(p => p.BaseRatePercentage)
            .HasPrecision(18, 4);

        modelBuilder.Entity<PolicyProduct>()
            .Property(p => p.MaxCoverageAmount)
            .HasPrecision(18, 2);

        // PolicyApplication → PolicyProduct
        modelBuilder.Entity<PolicyApplication>()
            .HasOne(a => a.Product)
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // PolicyApplication → Customer (User)
        modelBuilder.Entity<PolicyApplication>()
            .HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // PolicyApplication → AssignedAgent (User, nullable)
        modelBuilder.Entity<PolicyApplication>()
            .HasOne(a => a.AssignedAgent)
            .WithMany()
            .HasForeignKey(a => a.AssignedAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PolicyApplication>()
            .Property(a => a.MarketValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PolicyApplication>()
            .Property(a => a.RequestedCoverageAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PolicyApplication>()
            .Property(a => a.CalculatedPremium)
            .HasPrecision(18, 2);

        // Policy → PolicyApplication (optional)
        modelBuilder.Entity<Policy>()
            .HasOne(p => p.Application)
            .WithMany()
            .HasForeignKey(p => p.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Commission → Policy
        modelBuilder.Entity<Commission>()
            .HasOne(c => c.Policy)
            .WithMany()
            .HasForeignKey(c => c.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Commission>()
            .Property(c => c.CommissionRate)
            .HasPrecision(18, 4);

        modelBuilder.Entity<Commission>()
            .Property(c => c.CommissionAmount)
            .HasPrecision(18, 2);

        // Global UTC DateTime conversion to fix timezone issues
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }
    }
}