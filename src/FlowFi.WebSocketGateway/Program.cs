using FlowFi.Common.Authentication;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlowFiJwt(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHub<RealtimeHub>("/realtime");

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
