using FlowFi.WebSocketGateway.Config;
using Microsoft.Extensions.Options;

namespace FlowFi.WebSocketGateway.Sessions;

public sealed class VoiceSessionCleanupService(
    IVoiceSessionStore sessionStore,
    IOptions<VoiceRealtimeOptions> options,
    ILogger<VoiceSessionCleanupService> logger) : BackgroundService
{
    private readonly VoiceRealtimeOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var removed = sessionStore.RemoveExpired(
                TimeSpan.FromMinutes(Math.Max(1, _options.SessionTimeoutMinutes)));
            if (removed > 0)
            {
                logger.LogInformation("Removed {SessionCount} expired voice sessions.", removed);
            }
        }
    }
}
