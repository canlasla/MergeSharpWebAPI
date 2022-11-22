using System.Threading.Tasks;
using MergeSharpWebAPI.Hubs.Clients;
using MergeSharpWebAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace MergeSharpWebAPI.Hubs;

public static class UserHandler
{
    public static HashSet<string> ConnectedIds = new HashSet<string>();
}

public class FrontEndHub : Hub<IFrontEndClient>
{
    public override Task OnConnectedAsync()
    {
        UserHandler.ConnectedIds.Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        UserHandler.ConnectedIds.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
