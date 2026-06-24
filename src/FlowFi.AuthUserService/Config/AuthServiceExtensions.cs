using FlowFi.AuthUserService.Database;
using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Interface;
using FlowFi.AuthUserService.Repositories;
using FlowFi.AuthUserService.Security;
using FlowFi.AuthUserService.Services;
using FlowFi.AuthUserService.Validators;
using FlowFi.Common.Api;
using FlowFi.Common.Authentication;
using FlowFi.Common.OpenApi;
using FlowFi.Common.Persistence;
using FlowFi.EventBus.Messaging;
using FluentValidation;

namespace FlowFi.AuthUserService.Config;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFlowFiPostgres<AuthDbContext>(configuration);
        services.AddFlowFiJwt(configuration);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<RabbitMqPublisher>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, Services.AuthService>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<GoogleLoginRequest>, GoogleLoginRequestValidator>();
        services.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();
        services.AddScoped<IValidator<LogoutRequest>, LogoutRequestValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
        services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
        services.AddScoped<IValidator<UpdateProfileRequest>, UpdateProfileRequestValidator>();
        services.AddScoped<IValidator<UpdatePreferencesRequest>, UpdatePreferencesRequestValidator>();
        services.AddControllers().AddFlowFiApiBehavior();
        services.AddFlowFiSwagger();
        return services;
    }
}
