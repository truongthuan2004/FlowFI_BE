using System.Text;
using FlowFi.FinanceCoreService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.FinanceCoreService.Database;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<InternalTransfer> InternalTransfers => Set<InternalTransfer>();
    public DbSet<WalletBalanceLog> WalletBalanceLogs => Set<WalletBalanceLog>();
    public DbSet<SyncQueue> SyncQueue => Set<SyncQueue>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();
    public DbSet<FinanceAuditLog> FinanceAuditLogs => Set<FinanceAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureWallet(modelBuilder);
        ConfigureTag(modelBuilder);
        ConfigureTransaction(modelBuilder);
        ConfigureInternalTransfer(modelBuilder);
        ConfigureWalletBalanceLog(modelBuilder);
        ConfigureSyncQueue(modelBuilder);
        ConfigureRecurringTransaction(modelBuilder);
        ConfigureFinanceAuditLog(modelBuilder);

        // PostgreSQL identifiers in the supplied schema use snake_case.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static void ConfigureWallet(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Wallet>();
        entity.ToTable("wallets");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.WalletType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Balance).HasColumnType("numeric(18,2)");
        entity.Property(x => x.Balance).HasDefaultValue(0m);
        entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.HasIndex(x => x.UserId).HasDatabaseName("ix_wallets_user_id");
    }

    private static void ConfigureTag(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Tag>();
        entity.ToTable("tags");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Icon).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Color).HasMaxLength(20).IsRequired();
        entity.HasIndex(x => x.UserId).HasDatabaseName("ix_tags_user_id");
    }

    private static void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Transaction>();
        entity.ToTable("transactions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Note).IsRequired();
        entity.Property(x => x.Source).HasMaxLength(30).IsRequired();
        entity.Property(x => x.SyncStatus).HasMaxLength(20).IsRequired();
        entity.HasIndex(x => new { x.UserId, x.TransactionDate })
            .HasDatabaseName("ix_transactions_user_date")
            .IsDescending(false, true);
        entity.HasIndex(x => x.WalletId).HasDatabaseName("ix_transactions_wallet_id");
        entity.HasOne(x => x.Wallet)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Tag)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureInternalTransfer(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InternalTransfer>();
        entity.ToTable("internal_transfers", table =>
            table.HasCheckConstraint(
                "ck_internal_transfers_distinct_wallets",
                "from_wallet_id <> to_wallet_id"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        entity.Property(x => x.Note).IsRequired();
        entity.Property(x => x.SyncStatus).HasMaxLength(20).IsRequired();
        entity.HasIndex(x => new { x.UserId, x.TransferDate })
            .HasDatabaseName("ix_internal_transfers_user_date")
            .IsDescending(false, true);
        entity.HasOne(x => x.FromWallet)
            .WithMany(x => x.OutgoingTransfers)
            .HasForeignKey(x => x.FromWalletId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ToWallet)
            .WithMany(x => x.IncomingTransfers)
            .HasForeignKey(x => x.ToWalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureWalletBalanceLog(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<WalletBalanceLog>();
        entity.ToTable("wallet_balance_logs");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.OldBalance).HasColumnType("numeric(18,2)");
        entity.Property(x => x.ChangeAmount).HasColumnType("numeric(18,2)");
        entity.Property(x => x.NewBalance).HasColumnType("numeric(18,2)");
        entity.Property(x => x.Reason).HasMaxLength(50).IsRequired();
        entity.HasIndex(x => new { x.WalletId, x.CreatedAt })
            .HasDatabaseName("ix_wallet_balance_logs_wallet_created")
            .IsDescending(false, true);
        entity.HasOne(x => x.Wallet)
            .WithMany(x => x.BalanceLogs)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Transaction)
            .WithMany(x => x.BalanceLogs)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Transfer)
            .WithMany(x => x.BalanceLogs)
            .HasForeignKey(x => x.TransferId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureSyncQueue(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<SyncQueue>();
        entity.ToTable("sync_queue");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EntityType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Action).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Payload).HasColumnType("jsonb");
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
        entity.Property(x => x.RetryCount).HasDefaultValue(0);
        entity.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("ix_sync_queue_status_created");
    }

    private static void ConfigureRecurringTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RecurringTransaction>();
        entity.ToTable("recurring_transactions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Note).IsRequired();
        entity.Property(x => x.Frequency).HasMaxLength(20).IsRequired();
        entity.Property(x => x.StartDate).HasColumnType("date");
        entity.Property(x => x.EndDate).HasColumnType("date");
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.HasIndex(x => new { x.IsActive, x.NextRunAt })
            .HasDatabaseName("ix_recurring_transactions_next_run");
        entity.HasOne(x => x.Wallet)
            .WithMany(x => x.RecurringTransactions)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Tag)
            .WithMany(x => x.RecurringTransactions)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureFinanceAuditLog(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FinanceAuditLog>();
        entity.ToTable("finance_audit_logs");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.EntityType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Action).HasMaxLength(20).IsRequired();
        entity.Property(x => x.OldData).HasColumnType("jsonb");
        entity.Property(x => x.NewData).HasColumnType("jsonb");
        entity.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt })
            .HasDatabaseName("ix_finance_audit_logs_entity")
            .IsDescending(false, false, true);
    }

    private static string ToSnakeCase(string value)
    {
        var builder = new StringBuilder(value.Length + 5);
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (char.IsUpper(character) && i > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}

