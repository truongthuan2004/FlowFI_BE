using FlowFi.Common.Authentication;
using FlowFi.Common.Configuration;
using FlowFi.WebSocketGateway.Clients;
using FlowFi.WebSocketGateway.Config;
using FlowFi.WebSocketGateway.Hubs;
using FlowFi.WebSocketGateway.Sessions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;

EnvironmentFile.Load("WEBSOCKET");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlowFiJwt(builder.Configuration);
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events ??= new JwtBearerEvents();
    options.Events.OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        if (!string.IsNullOrWhiteSpace(accessToken) &&
            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    };
});
builder.Services.AddOptions<VoiceRealtimeOptions>()
    .Bind(builder.Configuration.GetSection(VoiceRealtimeOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.AiServiceBaseUrl, UriKind.Absolute, out _),
        "VoiceRealtime:AiServiceBaseUrl must be an absolute URL.")
    .Validate(options => options.TranscribeEveryChunks > 0,
        "VoiceRealtime:TranscribeEveryChunks must be greater than zero.")
    .Validate(options => options.MaxChunkSizeBytes > 0 && options.MaxSessionSizeBytes >= options.MaxChunkSizeBytes,
        "VoiceRealtime audio size limits are invalid.")
    .ValidateOnStart();
var realtimeOptions = builder.Configuration
    .GetSection(VoiceRealtimeOptions.SectionName)
    .Get<VoiceRealtimeOptions>() ?? new VoiceRealtimeOptions();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = (long)Math.Ceiling(realtimeOptions.MaxChunkSizeBytes * 4d / 3d) + 16 * 1024;
});
builder.Services.AddSingleton<IVoiceSessionStore, VoiceSessionStore>();
builder.Services.AddHostedService<VoiceSessionCleanupService>();
builder.Services.AddHttpClient<IAiVoiceClient, AiVoiceClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHub<RealtimeHub>("/realtime");
app.MapHub<VoiceTransactionHub>("/hubs/voice-transaction");

app.Run();

public sealed class RealtimeHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("connected", new { connectionId = Context.ConnectionId });
        await base.OnConnectedAsync();
    }

    public Task SubscribeUser(string userId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
    }

    public Task BroadcastNotification(string userId, object payload)
    {
        return Clients.Group($"user:{userId}").SendAsync("notification.received", payload);
    }
}
