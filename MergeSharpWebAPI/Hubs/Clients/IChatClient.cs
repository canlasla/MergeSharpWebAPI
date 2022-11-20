using System.Threading.Tasks;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Hubs.Clients;

public interface IChatClient
{
    Task ReceiveMessage(MergeSharpWebAPI.Models.LWWSet<int> lwwSet);
}
