using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Database;

public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>().ToTable("wallets").HasKey(x => x.Id);
        modelBuilder.Entity<Category>().ToTable("categories").HasKey(x => x.Id);
        modelBuilder.Entity<Tag>().ToTable("tags").HasKey(x => x.Id);
        modelBuilder.Entity<Transaction>().ToTable("transactions").HasKey(x => x.Id);
        modelBuilder.Entity<Transaction>().HasIndex(x => new { x.UserId, x.OccurredAt });
    }
}

