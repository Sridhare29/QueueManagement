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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(x => x.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<QueueToken>()
            .HasIndex(x => x.TokenNo)
            .IsUnique();
    }
}