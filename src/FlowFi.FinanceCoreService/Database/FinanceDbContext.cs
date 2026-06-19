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
    public DbSet<SyncQueueItem> SyncQueue => Set<SyncQueueItem>();
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
        entity.Property(x => x.Balance).HasColumnType("numeric(18,2)");
    }

    private static void ConfigureTag(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Tag>();
        entity.ToTable("tags");
        entity.HasKey(x => x.Id);
    }

    private static void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Transaction>();
        entity.ToTable("transactions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
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
        entity.ToTable("internal_transfers");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
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
        var entity = modelBuilder.Entity<SyncQueueItem>();
        entity.ToTable("sync_queue");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Payload).HasColumnType("jsonb");
    }

    private static void ConfigureRecurringTransaction(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RecurringTransaction>();
        entity.ToTable("recurring_transactions");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        entity.Property(x => x.StartDate).HasColumnType("date");
        entity.Property(x => x.EndDate).HasColumnType("date");
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
        entity.Property(x => x.OldData).HasColumnType("jsonb");
        entity.Property(x => x.NewData).HasColumnType("jsonb");
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

