using MergeSharpWebAPI.Models;
using MergeSharpWebAPI.Services;

namespace MergeSharpWebAPI.Hubs.Clients;

public interface IFrontEndClient
{
    Task ReceiveMessage(LWWSetModel<int> message);
    // TODO: add ReceiveMessage method for JSON containing node AND link data
    Task ReceiveMessage(List<Node> message);
    Task ReceiveMessage(GraphService.GraphInfo graph);
    Task ReceiveMessageTest(string message);
}
