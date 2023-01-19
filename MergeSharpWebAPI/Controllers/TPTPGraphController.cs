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

    // [HttpPut("SendTPTPGraphToFrontEnd")]
    // public async Task<ActionResult> SendMessage([FromBody] MergeSharpWebAPI.Models.TPTPGraph message)
    // {
    //     await _hubContext.Clients.All.ReceiveMessage(message);
    //     return NoContent();
    // }

    // Get all LWW Sets
    [HttpGet("GetAllTPTPGraphs")]
    public ActionResult<string> GetAll() => JsonConvert.SerializeObject(myTPTPGraphService.GetAll());

    // Get LWW Set by id
    [HttpGet("GetTPTPGraph/{id}")]
    public ActionResult<string> Get(int id)
    {
        var TPTPGraph = myTPTPGraphService.Get(id);

        if (TPTPGraph == null)
            return NotFound();

        return JsonConvert.SerializeObject(TPTPGraph);
    }

    // Create LWW Set
    [HttpPost("CreateTPTPGraph")]
    // using httprepl
    //post -h Content-Type=application/json -c "{"Id":<new id>, "TPTPGraph":[x, y, z]}"
    public IActionResult Create(MergeSharpWebAPI.Models.TPTPGraph mytptpGraph)
    {
        myTPTPGraphService.Add(mytptpGraph);
        return CreatedAtAction(nameof(Create), new { id = mytptpGraph.Id }, mytptpGraph);
    }

    // // Delete an LWW Set
    // [HttpDelete("DeleteTPTPGraph/{id}")]
    // public IActionResult Delete(int id)
    // {
    //     var TPTPGraph = myTPTPGraphService.Get(id);

    //     if (TPTPGraph is null)
    //         return NotFound();

    //     myTPTPGraphService.Delete(id);

    //     return NoContent();
    // }


    // //Check if LWW Set contains element
    // [HttpGet("ContainsVertex/{id}/{element}")]
    // public ActionResult<string> ContainsVertex(int id, Guid element)
    // {
    //     var TPTPGraph = myTPTPGraphService.Get(id);

    //     if (TPTPGraph == null)
    //         return NotFound();

    //     return JsonConvert.SerializeObject(myTPTPGraphService.TPTPGraphContains(id, element));
    // }

    // [HttpGet("ContainsEdge/{id}/{element}")]
    // public ActionResult<string> ContainsEdge(int id, Guid element)
    // {
    //     var TPTPGraph = myTPTPGraphService.Get(id);

    //     if (TPTPGraph == null)
    //         return NotFound();

    //     return JsonConvert.SerializeObject(myTPTPGraphService.TPTPGraphContains(id, element));
    // }


    // // Add an element to LWW Set
    // // put -h Content-Type=application/json -c "5"
    // [HttpPut("AddVertex/{id}")]
    // public async Task<IActionResult> AddVertex(int id, [FromBody] Guid newElement)
    // {
    //     var existingTPTPGraph = myTPTPGraphService.Get(id);
    //     if (existingTPTPGraph is null)
    //         return NotFound();

    //     myTPTPGraphService.AddVertex(id, newElement);

    //     Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
    //     Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(id)));
    //     if (connection.State == HubConnectionState.Connected)
    //     {
    //         await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
    //     }
    //     Console.WriteLine("Raised RecieveMessage event on all clients");
    //     return NoContent();
    // }
    // // Remove an element from LWW Set
    // [HttpPut("RemoveVertex/{id}")]
    // public async Task<IActionResult> RemoveVertex(int id, [FromBody] Guid element)
    // {
    //     var existingTPTPGraph = myTPTPGraphService.Get(id);
    //     if (existingTPTPGraph is null)
    //         return NotFound();

    //     if (!myTPTPGraphService.RemoveVertex(id, element))
    //         return NotFound();

    //     Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(id)));
    //     if (connection.State == HubConnectionState.Connected)
    //     {
    //         await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
    //     }
    //     Console.WriteLine("Raised RecieveMessage event on all clients");
    //     return NoContent();
    // }

    // //Merge set with id2 into set with id1
    // // put -h Content-Type=application/json -c "id2"
    // [HttpPut("Merge/{id1}")]
    // public async Task<IActionResult> Merge(int id1, [FromBody] int id2)
    // {
    //     var existingTPTPGraph1 = myTPTPGraphService.Get(id1);
    //     var existingTPTPGraph2 = myTPTPGraphService.Get(id2);

    //     if (existingTPTPGraph1 is null)
    //         return NotFound();
    //     else if (existingTPTPGraph2 is null)
    //         return NotFound();

    //     myTPTPGraphService.MergeTPTPGraphs(id1, id2);
    //     if (connection.State == HubConnectionState.Connected)
    //     {
    //         await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
    //     }
    //     return NoContent();
    // }
}
