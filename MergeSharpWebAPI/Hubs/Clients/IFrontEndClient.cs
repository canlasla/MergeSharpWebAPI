using System.Threading.Tasks;
using MergeSharpWebAPI.Models;

namespace MergeSharpWebAPI.Hubs.Clients;

public interface IFrontEndClient
{
    Task ReceiveMessage(FrontEndMessage message);
}
