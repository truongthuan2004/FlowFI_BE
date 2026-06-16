using FlowFi.AIProcessingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AIProcessingService.Database;

public sealed class AiDbContext(DbContextOptions<AiDbContext> options) : DbContext(options)
{
    public DbSet<AiJob> AiJobs => Set<AiJob>();
    public DbSet<AiInsight> Insights => Set<AiInsight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiJob>().ToTable("ai_jobs").HasKey(x => x.Id);
        modelBuilder.Entity<AiInsight>().ToTable("ai_insights").HasKey(x => x.Id);
    }
}

