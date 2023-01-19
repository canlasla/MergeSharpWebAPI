using System.Threading.Tasks;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Hubs.Clients;

public interface IFrontEndClient
{
    Task ReceiveMessage(LWWSet<int> message);
    Task ReceiveMessage(TPTPGraph message);
    Task ReceiveMessageTest(string message);
}
