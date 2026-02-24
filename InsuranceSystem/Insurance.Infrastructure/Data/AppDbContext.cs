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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Customer → User (1:1)
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Customer → Assigned Agent
        modelBuilder.Entity<Customer>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.AssignedAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Policy → Customer
        modelBuilder.Entity<Policy>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(p => p.CustomerId);

        // Policy → Property
        modelBuilder.Entity<Policy>()
            .HasOne<Property>()
            .WithMany()
            .HasForeignKey(p => p.PropertyId);

        // Claim → Policy
        modelBuilder.Entity<Claim>()
            .HasOne<Policy>()
            .WithMany()
            .HasForeignKey(c => c.PolicyId);

        // Payment → Policy
        modelBuilder.Entity<Payment>()
            .HasOne<Policy>()
            .WithMany()
            .HasForeignKey(p => p.PolicyId);
    }
}