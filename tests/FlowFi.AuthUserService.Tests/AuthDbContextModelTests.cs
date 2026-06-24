using FlowFi.AuthUserService.Database;
using FlowFi.AuthUserService.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowFi.AuthUserService.Tests;

public sealed class AuthDbContextModelTests
{
    [Theory]
    [InlineData(typeof(RefreshToken))]
    [InlineData(typeof(PasswordResetToken))]
    [InlineData(typeof(UserDevice))]
    [InlineData(typeof(UserLog))]
    public void User_owned_entities_use_users_as_fk_principal(Type entityType)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql("Host=localhost;Database=flowfi_auth_model_tests;Username=test;Password=test")
            .Options;
        using var db = new AuthDbContext(options);

        var modelEntity = db.Model.FindEntityType(entityType)!;
        var userIdForeignKey = modelEntity.GetForeignKeys().Single(x => x.Properties.Any(p => p.Name == "UserId"));

        Assert.Equal(typeof(User), userIdForeignKey.PrincipalEntityType.ClrType);
    }
}
