using FreelanceMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarket.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.Name).HasMaxLength(100);
        });

        // Project
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).HasMaxLength(200);
            e.HasOne(p => p.Customer)
             .WithMany(u => u.CustomerProjects)
             .HasForeignKey(p => p.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Freelancer)
             .WithMany(u => u.FreelancerProjects)
             .HasForeignKey(p => p.FreelancerId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Proposal
        modelBuilder.Entity<Proposal>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Project)
             .WithMany(pr => pr.Proposals)
             .HasForeignKey(p => p.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Freelancer)
             .WithMany(u => u.Proposals)
             .HasForeignKey(p => p.FreelancerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Review
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.FromUser)
             .WithMany(u => u.ReviewsGiven)
             .HasForeignKey(r => r.FromUserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ToUser)
             .WithMany(u => u.ReviewsReceived)
             .HasForeignKey(r => r.ToUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
