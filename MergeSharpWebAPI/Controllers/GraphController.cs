using MergeSharpWebAPI.Hubs;
using MergeSharpWebAPI.Hubs.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static MergeSharpWebAPI.Globals;
using Microsoft.AspNetCore.SignalR.Client;

namespace MergeSharpWebAPI.Controllers;


// https://stackoverflow.com/questions/12072277/reading-fromuri-and-frombody-at-the-same-time
// https://www.strathweb.com/2013/04/asp-net-web-api-parameter-binding-part-1-understanding-binding-from-uri/
[ApiController]
[Route("[controller]")]
public class GraphController : ControllerBase
{
    [HttpGet("vertices")]
    public ActionResult<string> Vertices(int? id = null)
    {
        if (id == null)
        {
            return JsonConvert.SerializeObject(myGraphService.Vertices);
        }
        // TODO: report 404 not found if myGraphService.Vertex() doesn't have the vertex
        return JsonConvert.SerializeObject(myGraphService.Vertex((int) id));
    }

    // Add a vertex to TPTP Graph
    [HttpPost("vertices")]
    public async Task<IActionResult> AddVertex(int id, int x, int y, string type)
    {
        // TODO: can this function only take in int?
        // TODO: does the front end use decimals or ints for position
        // TODO: need to do some conversion here if so
        if (myGraphService.AddVertex(id, x, y, type))
        {
            Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
            }
            Console.WriteLine("Raised AddVertex event on all clients");

            // TODO: return something related to the AddVertex success or failure above
            // https://www.restapitutorial.com/lessons/httpmethods.html
            // https://stackoverflow.com/questions/23892341/how-can-i-code-a-created-201-response-using-ihttpactionresult
            // this.CreatedAtRoute()
            return NoContent();
        }
        else
        {
            // TODO: return something related to the AddVertex success or failure above
            return NoContent();
        }

    }

    [HttpDelete("vertices")]
    public async Task<IActionResult> RemoveVertex(int id)
    {
        if (myGraphService.RemoveVertex(id))
        {
            Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
            }
            Console.WriteLine("Raised RemoveVertex event on all clients");
        }
        else
        {

        }

        // TODO: return something related to the RemoveVertex success or failure above
        return NoContent();
    }

    [HttpGet("edges")]
    public ActionResult<string> Edges(int? src = null, int? dst = null)
    {
        if (src == null || dst == null)
        {
            return JsonConvert.SerializeObject(myGraphService.EdgeCounts);
        }
        return JsonConvert.SerializeObject(myGraphService.EdgeCount((int) src, (int) dst));
    }

    [HttpPost("edges")]
    public async Task<IActionResult> AddEdge(int src, int dst)
    {
        if (myGraphService.AddEdge(src, dst))
        {
            Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
            }
            Console.WriteLine("Raised AddEdge event on all clients");

            // TODO: return something related to the AddEdge success or failure above
            return NoContent();
        }
        else
        {
            // TODO: return something related to the AddEdge success or failure above
            return NoContent();
        }
    }

    [HttpDelete("edges")]
    public async Task<IActionResult> RemoveEdge(int src, int dst)
    {
        if (myGraphService.RemoveEdge(src, dst))
        {
            Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
            }
            Console.WriteLine("Raised RemoveEdge event on all clients");

            // TODO: return something related to the RemoveEdge success or failure above
            return NoContent();
        }
        else
        {
            // TODO: return something related to the RemoveEdge success or failure above
            return NoContent();
        }
    }
}
