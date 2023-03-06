using MergeSharpWebAPI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static MergeSharpWebAPI.ServerConnection.Globals;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MergeSharpWebAPI.Controllers;


// https://stackoverflow.com/questions/12072277/reading-fromuri-and-frombody-at-the-same-time
// https://www.strathweb.com/2013/04/asp-net-web-api-parameter-binding-part-1-understanding-binding-from-uri/

// Error response codes follow:
// https://www.restapitutorial.com/lessons/httpmethods.html
[ApiController]
[Route("graph")]
public class GraphController : ControllerBase
{
    [HttpGet("vertices")]
    public ActionResult<string> Vertices([FromQuery] int? key = null)
    {
        if (key == null)
        {
            return JsonConvert.SerializeObject(myGraphService.Vertices);
        }

        try
        {
            return JsonConvert.SerializeObject(myGraphService.Vertex((int) key));
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost("vertices")]
    public async Task<IActionResult> AddVertex([BindRequired, FromQuery] int key, [BindRequired, FromQuery] string loc, [BindRequired, FromQuery] string category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        string[] position = loc.Split();

        if (position.Length != 2 || !int.TryParse(position[0], out int x) || !int.TryParse(position[1], out int y))
        {
            return BadRequest();
        }

        if (myGraphService.AddVertex(key, x, y, category))
        {
            Console.WriteLine($"Added Vertex {key} ({loc}) {category} locally");

            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
                Console.WriteLine("Raised SendEncodedMessage event with new state on server to propagate to other clients");
            }

            return Ok();
        }
        else
        {
            return Conflict();
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
            Console.WriteLine($"Removed Vertex {key} locally");

            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
                Console.WriteLine("Raised SendEncodedMessage event with new state on server to propagate to other clients");
            }

            return Ok();
        }
        else
        {
            return NotFound();
        }
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
            try
            {
                return JsonConvert.SerializeObject(myGraphService.EdgeCount((int) srcKey, (int) dstKey));
            }
            catch
            {
                return NotFound();
            }
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
            Console.WriteLine($"Added Edge ({srcKey}, {dstKey}) locally");
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
                Console.WriteLine("Raised SendEncodedMessage event with new state on server to propagate to other clients");
            }

            return Ok();
        }
        else
        {
            return NotFound();
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
            Console.WriteLine($"Removed Edge ({srcKey}, {dstKey}) locally");
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
                Console.WriteLine("Raised SendEncodedMessage event with new state on server to propagate to other clients");
            }

            return Ok();
        }
        else
        {
            return NotFound();
        }
    }
}
