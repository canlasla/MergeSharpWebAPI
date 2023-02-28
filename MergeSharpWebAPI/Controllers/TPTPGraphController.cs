using MergeSharp;
using MergeSharpWebAPI.Models;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static MergeSharpWebAPI.Globals;
using MergeSharpWebAPI.Hubs;
using MergeSharpWebAPI.Hubs.Clients;
using Microsoft.AspNetCore.SignalR.Client;

namespace MergeSharpWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TPTPGraphController : ControllerBase
{
    private readonly IHubContext<FrontEndHub, IFrontEndClient> _hubContext;

    public TPTPGraphController(IHubContext<FrontEndHub, IFrontEndClient> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPut("SendTPTPGraphToFrontEnd")]
    public async Task<ActionResult> SendMessage([FromBody] List<Node> message)
    {
        // TODO: change the message to JSON for frontend
        await _hubContext.Clients.All.ReceiveMessage(message);
        return NoContent();
    }

    // Get all TPTP Graphs
    [HttpGet("GetAllTPTPGraphs")]
    public ActionResult<string> GetAll() => JsonConvert.SerializeObject(myTPTPGraphService.GetAll());

    // Get TPTP Graph by id
    [HttpGet("GetTPTPGraph/{id}")]
    public ActionResult<string> Get(int id)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph == null)
            return NotFound();

        return JsonConvert.SerializeObject(TPTPGraph);
    }

    // Create TPTP Graph
    [HttpPost("CreateTPTPGraph")]
    // using httprepl
    //post -h Content-Type=application/json -c "<a new id here>"
    public IActionResult Create([FromBody] int id)
    {
        myTPTPGraphService.Add(new TPTPGraphModel { Id = id, TptpGraph = new TPTPGraph() });
        return CreatedAtAction(nameof(Create), new { Id = id }, myTPTPGraphService.Get(id));
    }

    // Delete a TPTP Graph
    [HttpDelete("DeleteTPTPGraph/{id}")]
    public IActionResult Delete(int id)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph is null)
            return NotFound();

        myTPTPGraphService.Delete(id);

        return NoContent();
    }

    //Check if TPTP Graph contains vertex
    [HttpGet("ContainsVertex/{id}/{v}")]
    public ActionResult<string> ContainsVertex(int id, string v)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph == null)
            return NotFound();

        return JsonConvert.SerializeObject(myTPTPGraphService.TPTPGraphContains(id, new Guid(v)));
    }

    [HttpGet("ContainsEdge/{id}/{v1}/{v2}")]
    public ActionResult<string> ContainsEdge(int id, string v1, string v2)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph == null)
            return NotFound();

        return JsonConvert.SerializeObject(myTPTPGraphService.TPTPGraphContains(id, new Guid(v1), new Guid(v2)));
    }

    [HttpGet("LookupVertices/{id}")]
    public ActionResult<string> LookupVertices(int id)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph == null)
            return NotFound();

        return JsonConvert.SerializeObject(myTPTPGraphService.LookupVertices(id));
    }

    [HttpGet("LookupEdges/{id}")]
    public ActionResult<string> LookupEdges(int id)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph == null)
            return NotFound();

        return JsonConvert.SerializeObject(myTPTPGraphService.LookupEdges(id));
    }

    [HttpGet("LookupNodes/{id}")]
    public ActionResult<string> LookupNodes(int id)
    {
        string [] types = { "and", "or", "not", "xor", "nand", "nor" };

        var result = new List<Node>();

        foreach (KeyValuePair<Guid, int> entry in IDMapping)
        {
            Random rnd = new Random();

            var n = new Node(types[rnd.Next(0, types.Length)], entry.Value, $"{Math.Pow(-1, rnd.Next(1,3)) * rnd.Next(0, 200)} {Math.Pow(-1, rnd.Next(1, 3)) * rnd.Next(0, 200)}");
            result.Add(n);
        }

        return JsonConvert.SerializeObject(result);
    }

    // Add a vertex to TPTP Graph
    [HttpGet("AddVertex/{id}")]
    public async Task<IActionResult> AddVertex(int id)
    {
        var existingTPTPGraph = myTPTPGraphService.Get(id);
        if (existingTPTPGraph is null)
            return NotFound();

        myTPTPGraphService.AddVertex(id, Guid.NewGuid());

        Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
        Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(id)));
        if (connection.State == HubConnectionState.Connected)
        {
            await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
        }
        Console.WriteLine("Raised RecieveMessage event on all clients");
        return NoContent();
    }

    // Remove a vertex from TPTP Graph
    [HttpPut("RemoveVertex/{id}")]
    // put -h Content-Type=application/json -c ""f628ec6f-936c-4277-bfce-2e220df11fa1""
    public async Task<IActionResult> RemoveVertex(int id, [FromBody] string element)
    {
        var existingTPTPGraph = myTPTPGraphService.Get(id);
        if (existingTPTPGraph is null)
            return NotFound();

        if (!myTPTPGraphService.RemoveVertex(id, new Guid(element)))
            return NotFound();

        Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(id)));
        if (connection.State == HubConnectionState.Connected)
        {
            await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
        }
        Console.WriteLine("Raised RecieveMessage event on all clients");
        return NoContent();
    }

    // Add an edge to TPTP Graph
    // put -h Content-Type=application/json -c ""<v2 guid>""
    [HttpPut("AddEdge/{id}/{v1}")]
    public async Task<IActionResult> AddEdge(int id, string v1, [FromBody] string v2)
    {
        var existingTPTPGraph = myTPTPGraphService.Get(id);
        if (existingTPTPGraph is null)
            return NotFound();

        myTPTPGraphService.AddEdge(id, new Guid(v1), new Guid(v2));

        Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
        Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(id)));
        if (connection.State == HubConnectionState.Connected)
        {
            await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
        }
        Console.WriteLine("Raised RecieveMessage event on all clients");
        return NoContent();
    }

    // Remove an edge from TPTP Graph
    [HttpPut("RemoveEdge/{id}/{v1}")]
    // put -h Content-Type=application/json -c ""f628ec6f-936c-4277-bfce-2e220df11fa1""
    public async Task<IActionResult> RemoveEdge(int id, string v1, [FromBody] string v2)
    {
        var existingTPTPGraph = myTPTPGraphService.Get(id);
        if (existingTPTPGraph is null)
            return NotFound();

        if (!myTPTPGraphService.RemoveEdge(id, new Guid(v1), new Guid(v2)))
            return NotFound();

        Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(id)));
        if (connection.State == HubConnectionState.Connected)
        {
            await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
        }
        Console.WriteLine("Raised RecieveMessage event on all clients");
        return NoContent();
    }

    //Merge TPTP Graph with id2 into TPTP Graph with id1
    // put -h Content-Type=application/json -c "id2"
    [HttpPut("Merge/{id1}")]
    public async Task<IActionResult> Merge(int id1, [FromBody] int id2)
    {
        var existingTPTPGraph1 = myTPTPGraphService.Get(id1);
        var existingTPTPGraph2 = myTPTPGraphService.Get(id2);

        if (existingTPTPGraph1 is null)
            return NotFound();
        else if (existingTPTPGraph2 is null)
            return NotFound();

        myTPTPGraphService.MergeTPTPGraphs(id1, id2);
        if (connection.State == HubConnectionState.Connected)
        {
            await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
        }
        return NoContent();
    }
}
