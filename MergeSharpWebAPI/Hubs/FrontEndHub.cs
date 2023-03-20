using MergeSharpWebAPI.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace MergeSharpWebAPI.Hubs;

public static class UserHandler
{
    public static readonly HashSet<string> ConnectedIds = new();
}

public class FrontEndHub : Hub<IFrontEndClient>
{
}
