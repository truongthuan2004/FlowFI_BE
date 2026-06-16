using FlowFi.AuthUserService.Database;
using FlowFi.AuthUserService.Interface;
using FlowFi.AuthUserService.Repositories;
using FlowFi.AuthUserService.Services;
using FlowFi.EventBus.Messaging;
using FlowFi.Common.Persistence;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;

namespace FlowFi.AuthUserService.Config;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<AuthDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, Services.AuthService>();
        services.AddControllers();
        services.AddFlowFiSwagger();
        return services;
    }
}
