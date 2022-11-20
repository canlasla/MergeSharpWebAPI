using System.Threading.Tasks;
using MergeSharpWebAPI.Hubs.Clients;
using MergeSharpWebAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace MergeSharpWebAPI.Hubs;

public class ChatHub : Hub<IChatClient>
{
    public async Task SendMessage(MergeSharpWebAPI.Models.LWWSet<int> lwwSet)
    {
        await this.Clients.All.ReceiveMessage(lwwSet);
    }
}
