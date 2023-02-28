using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Hubs.Clients;

public interface IFrontEndClient
{
    Task ReceiveMessage(LWWSetModel<int> message);
    Task ReceiveMessage(List<Node> message);
    Task ReceiveMessageTest(string message);
}
