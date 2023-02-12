using MergeSharpWebAPI.Hubs;
using MergeSharpWebAPI.Hubs.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static MergeSharpWebAPI.Globals;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MergeSharpWebAPI.Controllers;


// https://stackoverflow.com/questions/12072277/reading-fromuri-and-frombody-at-the-same-time
// https://www.strathweb.com/2013/04/asp-net-web-api-parameter-binding-part-1-understanding-binding-from-uri/
[ApiController]
[Route("[controller]")]
public class GraphController : ControllerBase
{
    [HttpGet("vertices")]
    public ActionResult<string> Vertices([FromQuery] int? key = null)
    {
        if (key == null)
        {
            return JsonConvert.SerializeObject(myGraphService.Vertices);
        }
        return JsonConvert.SerializeObject(myGraphService.Vertex((int) key));
    }

    // Add a vertex to TPTP Graph
    [HttpPost("vertices")]
    public async Task<IActionResult> AddVertex([BindRequired, FromQuery] int key, [BindRequired, FromQuery] double x, [BindRequired, FromQuery] double y, [BindRequired, FromQuery] string type)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (myGraphService.AddVertex(key, x, y, type))
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
    public async Task<IActionResult> RemoveVertex([BindRequired, FromQuery] int key)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (myGraphService.RemoveVertex(key))
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
    public ActionResult<string> Edges([FromQuery] int? srcKey, [FromQuery] int? dstKey)
    {
        if (srcKey == null && dstKey == null)
        {
            return JsonConvert.SerializeObject(myGraphService.EdgeCounts);
        }
        else if (srcKey != null && dstKey != null)
        {
            return JsonConvert.SerializeObject(myGraphService.EdgeCount((int) srcKey, (int) dstKey));
        }

        return BadRequest();
    }

    [HttpPost("edges")]
    public async Task<IActionResult> AddEdge([BindRequired, FromQuery] int srcKey, [BindRequired, FromQuery] int dstKey)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (myGraphService.AddEdge(srcKey, dstKey))
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
    public async Task<IActionResult> RemoveEdge([BindRequired, FromQuery] int srcKey, [BindRequired, FromQuery] int dstKey)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (myGraphService.RemoveEdge(srcKey, dstKey))
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
