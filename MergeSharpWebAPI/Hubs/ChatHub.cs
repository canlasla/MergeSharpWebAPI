using System.Threading.Tasks;
using MergeSharpWebAPI.Hubs.Clients;
using MergeSharpWebAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace MergeSharpWebAPI.Hubs;

public class ChatHub : Hub<IChatClient>
{
    public async Task SendMessage(ChatMessage message)
    {
        await this.Clients.All.ReceiveMessage(message);
    }
}
