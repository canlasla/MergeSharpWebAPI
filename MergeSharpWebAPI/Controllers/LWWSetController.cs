using MergeSharp;
using MergeSharpWebAPI.Models;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static MergeSharpWebAPI.ServerConnection.Globals;
using MergeSharpWebAPI.Hubs;
using MergeSharpWebAPI.Hubs.Clients;
using Microsoft.AspNetCore.SignalR.Client;
using MergeSharpWebAPI.ServerConnection;

namespace MergeSharpWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LWWSetController : ControllerBase
{
    private readonly IHubContext<FrontEndHub, IFrontEndClient> _hubContext;

    public LWWSetController(IHubContext<FrontEndHub, IFrontEndClient> hubContext)
    {
        // NOTE: this client is the frontend
        _hubContext = hubContext;
    }

    [HttpPut("SendLWWSetToFrontEnd")]
    public async Task<ActionResult> SendMessage([FromBody] MergeSharpWebAPI.Models.LWWSetModel<int> message)
    {
        await _hubContext.Clients.All.ReceiveMessage(message);
        return NoContent();
    }

    // Get all LWW Sets
    [HttpGet("GetAllLWWSets")]
    public ActionResult<string> GetAll() => JsonConvert.SerializeObject(myLWWSetService.GetAll());

    // Get LWW Set by id
    [HttpGet("GetLWWSet/{id}")]
    public ActionResult<string> Get(int id)
    {
        var lwwSet = myLWWSetService.Get(id);

        if (lwwSet == null)
            return NotFound();

        return JsonConvert.SerializeObject(lwwSet);
    }

    // Create LWW Set
    [HttpPost("CreateLWWSet")]
    // using httprepl
    //post -h Content-Type=application/json -c "{"Id":<new id>, "LwwSet":[x, y, z]}"
    public IActionResult Create(MergeSharpWebAPI.Models.LWWSetModel<int> lwwSet)
    {
        myLWWSetService.Add(lwwSet);
        return CreatedAtAction(nameof(Create), new { id = lwwSet.Id }, lwwSet);
    }

    // Update an LWW Set
    [HttpPut("UpdateLWWSet/{id}")]
    //put -h Content-Type=application/json -c "{"Id":<some id, "LwwSet":[x, y, z]}"
    public async Task<ActionResult> Update(int id, MergeSharpWebAPI.Models.LWWSetModel<int> lwwSet)
    {
        if (id != lwwSet.Id)
            return BadRequest();

        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        myLWWSetService.Update(lwwSet);
        if (serverConnection.State == HubConnectionState.Connected)
        {
            await Globals.serverConnection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
        }
        return NoContent();
    }

    // Delete an LWW Set
    [HttpDelete("DeleteLWWSet/{id}")]
    public IActionResult Delete(int id)
    {
        var lwwSet = myLWWSetService.Get(id);

        if (lwwSet is null)
            return NotFound();

        myLWWSetService.Delete(id);

        return NoContent();
    }

    //Get count of LWW Set
    [HttpGet("CountLWWSet/{id}")]
    public ActionResult<string> CountLWWSet(int id)
    {
        var lwwSet = myLWWSetService.Get(id);

        if (lwwSet == null)
            return NotFound();

        return JsonConvert.SerializeObject(myLWWSetService.CountLWWSet(id));
    }

    //Check if LWW Set contains element
    [HttpGet("Contains/{id}/{element}")]
    public ActionResult<string> Contains(int id, int element)
    {
        var lwwSet = myLWWSetService.Get(id);

        if (lwwSet == null)
            return NotFound();

        return JsonConvert.SerializeObject(myLWWSetService.LWWSetContains(id, element));
    }


    // Add an element to LWW Set
    // put -h Content-Type=application/json -c "5"
    [HttpPut("AddElement/{id}")]
    public async Task<IActionResult> AddElement(int id, [FromBody] int newElement)
    {
        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        myLWWSetService.AddElement(id, newElement);

        Console.WriteLine(string.Join(", ", UserHandler.ConnectedIds.ToList()));
        Console.WriteLine(JsonConvert.SerializeObject(myLWWSetService.Get(id)));
        Console.WriteLine(serverConnection.State);
        if (serverConnection.State == HubConnectionState.Connected)
        {
            await Globals.serverConnection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
        }
        Console.WriteLine("Raised RecieveMessage event on all clients");
        return NoContent();
    }
    // Remove an element from LWW Set
    [HttpPut("RemoveElement/{id}")]
    public async Task<IActionResult> RemoveElement(int id, [FromBody] int element)
    {
        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        if (!myLWWSetService.RemoveElement(id, element))
            return NotFound();

        Console.WriteLine(JsonConvert.SerializeObject(myLWWSetService.Get(id)));
        Console.WriteLine(serverConnection.State);
        if (serverConnection.State == HubConnectionState.Connected)
        {
            await Globals.serverConnection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
        }
        Console.WriteLine("Raised RecieveMessage event on all clients");
        return NoContent();
    }

    // Clear an LWW Set
    [HttpDelete("ClearLWWSet/{id}")]
    public async Task<IActionResult> ClearLWWSet(int id)
    {
        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        myLWWSetService.ClearLWWSet(id);
        if (serverConnection.State == HubConnectionState.Connected)
        {
            await Globals.serverConnection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
        }
        return NoContent();
    }

    //Merge set with id2 into set with id1
    // put -h Content-Type=application/json -c "id2"
    [HttpPut("Merge/{id1}")]
    public async Task<IActionResult> Merge(int id1, [FromBody] int id2)
    {
        var existingLWWSet1 = myLWWSetService.Get(id1);
        var existingLWWSet2 = myLWWSetService.Get(id2);

        if (existingLWWSet1 is null)
            return NotFound();
        else if (existingLWWSet2 is null)
            return NotFound();

        myLWWSetService.MergeLWWSets(id1, id2);
        if (serverConnection.State == HubConnectionState.Connected)
        {
            await Globals.serverConnection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
        }
        return NoContent();
    }
}
