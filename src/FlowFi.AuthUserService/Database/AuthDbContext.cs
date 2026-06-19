using FlowFi.AuthUserService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AuthUserService.Database;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<UserLog> UserLogs => Set<UserLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Users
        modelBuilder.Entity<User>().ToTable("users").HasKey(x => x.Id);
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Role).HasDefaultValue("User").HasMaxLength(50);

        // Refresh Tokens
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens").HasKey(x => x.Id);
        modelBuilder.Entity<RefreshToken>()
            .HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Password Reset Tokens
        modelBuilder.Entity<PasswordResetToken>().ToTable("password_reset_tokens").HasKey(x => x.Id);
        modelBuilder.Entity<PasswordResetToken>()
            .HasOne<PasswordResetToken>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User Devices
        modelBuilder.Entity<UserDevice>().ToTable("user_devices").HasKey(x => x.Id);
        modelBuilder.Entity<UserDevice>()
            .HasOne<UserDevice>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User Logs
        modelBuilder.Entity<UserLog>().ToTable("user_logs").HasKey(x => x.Id);
        modelBuilder.Entity<UserLog>()
            .HasOne<UserLog>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
