using FlowFi.AnalyticsService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AnalyticsService.Database;

public sealed class ReportDbContext(DbContextOptions<ReportDbContext> options) : DbContext(options)
{
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<SavingStreak> SavingStreaks => Set<SavingStreak>();
    public DbSet<analyticsnapshot> analyticsnapshots => Set<analyticsnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>().ToTable("budgets").HasKey(x => x.Id);
        modelBuilder.Entity<Goal>().ToTable("goals").HasKey(x => x.Id);
        modelBuilder.Entity<SavingStreak>().ToTable("saving_streaks").HasKey(x => x.Id);
        modelBuilder.Entity<analyticsnapshot>().ToTable("report_snapshots").HasKey(x => x.Id);
    }
}

