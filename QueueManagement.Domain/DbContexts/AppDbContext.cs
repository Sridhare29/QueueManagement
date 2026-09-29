using Microsoft.EntityFrameworkCore;
using QueueManagement.Domain.Entities;

namespace QueueManagement.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Counter> Counters => Set<Counter>();
    public DbSet<QueueToken> QueueTokens => Set<QueueToken>();
    public DbSet<Queue> Queues => Set<Queue>();
    public DbSet<QueueMembership> QueueMemberships => Set<QueueMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<QueueToken>()
            .HasIndex(x => new { x.QueueId, x.TokenNo })
            .IsUnique();

        modelBuilder.Entity<Queue>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Queue>()
            .Property(x => x.AccessCode)
            .HasMaxLength(6);

        modelBuilder.Entity<Queue>()
            .HasIndex(x => x.AccessCode)
            .IsUnique();

        modelBuilder.Entity<Queue>()
            .HasOne(x => x.AdminUser)
            .WithMany()
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<QueueMembership>()
            .HasKey(x => new { x.QueueId, x.UserId });

        modelBuilder.Entity<QueueMembership>()
            .HasOne(x => x.Queue)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.QueueId);

        modelBuilder.Entity<QueueMembership>()
            .HasOne(x => x.User)
            .WithMany(x => x.QueueMemberships)
            .HasForeignKey(x => x.UserId);
    }
}