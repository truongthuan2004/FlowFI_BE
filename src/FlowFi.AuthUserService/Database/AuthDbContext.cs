using FlowFi.AuthUserService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.AuthUserService.Database;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OAuthIdentity> OAuthIdentities => Set<OAuthIdentity>();
    public DbSet<LoginDevice> LoginDevices => Set<LoginDevice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users").HasKey(x => x.Id);
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<OAuthIdentity>().ToTable("oauth_identities").HasKey(x => x.Id);
        modelBuilder.Entity<LoginDevice>().ToTable("login_devices").HasKey(x => x.Id);
    }
}

