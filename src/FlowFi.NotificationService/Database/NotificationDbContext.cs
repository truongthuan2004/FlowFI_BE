using FlowFi.NotificationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.NotificationService.Database;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PushDevice> PushDevices => Set<PushDevice>();
    public DbSet<DeviceSyncState> DeviceSyncStates => Set<DeviceSyncState>();
    public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Notifications
        modelBuilder.Entity<Notification>().ToTable("notifications").HasKey(x => x.Id);
        modelBuilder.Entity<Notification>().HasIndex(x => new { x.UserId, x.IsRead });
        modelBuilder.Entity<Notification>().HasIndex(x => x.CreatedAt);
        modelBuilder.Entity<Notification>().HasIndex(x => x.NotificationType);

        // Push Devices
        modelBuilder.Entity<PushDevice>().ToTable("push_devices").HasKey(x => x.Id);
        modelBuilder.Entity<PushDevice>().HasIndex(x => new { x.UserId, x.DeviceFingerprint }).IsUnique();
        modelBuilder.Entity<PushDevice>().HasIndex(x => x.Token);
        modelBuilder.Entity<PushDevice>().HasIndex(x => x.Platform);

        // Device Sync States
        modelBuilder.Entity<DeviceSyncState>().ToTable("device_sync_states").HasKey(x => x.Id);
        modelBuilder.Entity<DeviceSyncState>().HasIndex(x => new { x.UserId, x.DeviceFingerprint }).IsUnique();

        // Notification Settings
        modelBuilder.Entity<NotificationSetting>().ToTable("notification_settings").HasKey(x => x.Id);
        modelBuilder.Entity<NotificationSetting>().HasIndex(x => x.UserId).IsUnique();
    }
}
