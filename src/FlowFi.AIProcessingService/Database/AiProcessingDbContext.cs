using FlowFi.AIProcessingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AIProcessingService.Database;

public sealed class AiProcessingDbContext(DbContextOptions<AiProcessingDbContext> options) : DbContext(options)
{
    public DbSet<AiProcessingRequest> AiProcessingRequests => Set<AiProcessingRequest>();
    public DbSet<AiProcessingResult> AiProcessingResults => Set<AiProcessingResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<AiProcessingRequest>(entity =>
        {
            entity.ToTable("ai_processing_request", table =>
            {
                table.HasCheckConstraint("chk_ai_request_input_type", "input_type IN ('AUDIO', 'IMAGE', 'TEXT')");
                table.HasCheckConstraint("chk_ai_request_type", "request_type IN ('VOICE_TO_TEXT', 'OCR', 'SPENDING_ANALYSIS')");
                table.HasCheckConstraint("chk_ai_request_status", "status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.InputType).HasColumnName("input_type").HasMaxLength(20).IsRequired();
            entity.Property(x => x.RequestType).HasColumnName("request_type").HasMaxLength(50).IsRequired();
            entity.Property(x => x.InputUrl).HasColumnName("input_url").HasColumnType("text");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING").IsRequired();
            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
            entity.Property(x => x.CompletedAt)
                .HasColumnName("completed_at")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<AiProcessingResult>(entity =>
        {
            entity.ToTable("ai_processing_result", table =>
            {
                table.HasCheckConstraint("chk_ai_result_transaction_type", "transaction_type IN ('INCOME', 'EXPENSE')");
                table.HasCheckConstraint("chk_ai_result_tag", "tag IN ('FOOD', 'TRANSPORT', 'SHOPPING', 'EDUCATION')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(x => x.RequestId).HasColumnName("request_id").IsRequired();
            entity.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
            entity.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(20);
            entity.Property(x => x.Tag).HasColumnName("tag").HasMaxLength(50);
            entity.Property(x => x.TransactionDate)
                .HasColumnName("transaction_date")
                .HasColumnType("timestamp without time zone");
            entity.Property(x => x.RawResponse).HasColumnName("raw_response").HasColumnType("text");

            entity.HasIndex(x => x.RequestId).IsUnique();
            entity.HasOne(x => x.Request)
                .WithOne(x => x.Result)
                .HasForeignKey<AiProcessingResult>(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
