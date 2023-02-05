using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Hubs.Clients;

public interface IFrontEndClient
{
    Task ReceiveMessage(LWWSetModel<int> message);
    Task ReceiveMessage(TPTPGraphModel message);
    Task ReceiveMessageTest(string message);
}
