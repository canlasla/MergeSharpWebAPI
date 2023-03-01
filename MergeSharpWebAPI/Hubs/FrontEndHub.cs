using MergeSharpWebAPI.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace MergeSharpWebAPI.Hubs;

public static class UserHandler
{
    public static readonly HashSet<string> ConnectedIds = new();
}

public class FrontEndHub : Hub<IFrontEndClient>
{
    public override Task OnConnectedAsync()
    {
        _ = UserHandler.ConnectedIds.Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _ = UserHandler.ConnectedIds.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
