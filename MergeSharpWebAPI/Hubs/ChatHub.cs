using System.Threading.Tasks;
using Chatty.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Chatty.Api.Hubs.Clients;

// namespace Chatty.Api.Hubs
// {
//     public class ChatHub : Hub<IChatClient>
//     {
//         // Sends message to all clients listening to "RecieveMessage" event
//         public async Task SendMessage(ChatMessage message)
//         {
//             await this.Clients.All.ReceiveMessage(message);
//         }
//     }
// }

namespace Chatty.Api.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(ChatMessage message)
        {
            await this.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
