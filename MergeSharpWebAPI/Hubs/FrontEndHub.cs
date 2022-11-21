using System.Threading.Tasks;
using MergeSharpWebAPI.Hubs.Clients;
using MergeSharpWebAPI.Models;
using Microsoft.AspNetCore.SignalR;

namespace MergeSharpWebAPI.Hubs;

public class FrontEndHub : Hub<IFrontEndClient>
{
    public async Task SendMessage(FrontEndMessage message)
    {
        await this.Clients.All.ReceiveMessage(message);
    }
}
