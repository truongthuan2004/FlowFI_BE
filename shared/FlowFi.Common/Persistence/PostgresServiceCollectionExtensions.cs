using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowFi.Common.Persistence;

public static class PostgresServiceCollectionExtensions
{
    public static IServiceCollection AddFlowFiPostgres<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName)
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
}
