using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowFi.Common.Persistence;

public static class PostgresServiceCollectionExtensions
{
    public static IServiceCollection AddFlowFiPostgres<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Default")
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{connectionStringName} is required.");

        services.AddDbContext<TContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddHealthChecks().AddNpgSql(connectionString, name: "postgres");
        return services;
    }

    public static async Task MigrateDatabaseOnStartupAsync<TContext>(
        this WebApplication app,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        if (!app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
