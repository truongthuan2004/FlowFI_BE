using FlowFi.AuthUserService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AuthUserService.Database;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OAuthIdentity> OAuthIdentities => Set<OAuthIdentity>();
    public DbSet<LoginDevice> LoginDevices => Set<LoginDevice>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users").HasKey(x => x.Id);
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<OAuthIdentity>().ToTable("user_auth_identities").HasKey(x => x.Id);
        modelBuilder.Entity<LoginDevice>().ToTable("user_devices").HasKey(x => x.Id);
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens").HasKey(x => x.Id);
        modelBuilder.Entity<PasswordResetToken>().ToTable("password_reset_tokens").HasKey(x => x.Id);
    }
}
