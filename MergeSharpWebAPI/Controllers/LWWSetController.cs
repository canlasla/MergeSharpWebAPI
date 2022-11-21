using MergeSharp;
using MergeSharpWebAPI.Models;
using MergeSharpWebAPI.Services;
//using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static MergeSharpWebAPI.Globals;

namespace MergeSharpWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LWWSetController : ControllerBase
{
    public LWWSetController()
    {
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
    public IActionResult Create(MergeSharpWebAPI.Models.LWWSet<int> lwwSet)
    {
        myLWWSetService.Add(lwwSet);
        return CreatedAtAction(nameof(Create), new { id = lwwSet.Id }, lwwSet);
    }

    // Update an LWW Set
    [HttpPut("UpdateLWWSet/{id}")]
    //put -h Content-Type=application/json -c "{"Id":<some id, "LwwSet":[x, y, z]}"
    public IActionResult Update(int id, MergeSharpWebAPI.Models.LWWSet<int> lwwSet)
    {
        if (id != lwwSet.Id)
            return BadRequest();

        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        myLWWSetService.Update(lwwSet);

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
    public IActionResult AddElement(int id, [FromBody]int newElement)
    {
        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        myLWWSetService.AddElement(id, newElement);

        return NoContent();
    }
    // Remove an element from LWW Set
    [HttpPut("RemoveElement/{id}")]
    public IActionResult RemoveElement(int id, [FromBody] int element)
    {
        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        if(!myLWWSetService.RemoveElement(id, element))
            return NotFound();

        return NoContent();
    }

    // Clear an LWW Set
    [HttpDelete("ClearLWWSet/{id}")]
    public IActionResult ClearLWWSet(int id)
    {
        var existingLWWSet = myLWWSetService.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        myLWWSetService.ClearLWWSet(id);

        return NoContent();
    }

    //Merge set with id2 into set with id1
    // put -h Content-Type=application/json -c "id2"
    [HttpPut("Merge/{id1}")]
    public IActionResult Merge(int id1, [FromBody]int id2)
    {
        var existingLWWSet1 = myLWWSetService.Get(id1);
        var existingLWWSet2 = myLWWSetService.Get(id2);

        if (existingLWWSet1 is null)
            return NotFound();
        else if (existingLWWSet2 is null)
            return NotFound();

        myLWWSetService.MergeLWWSets(id1, id2);

        return NoContent();
    }
}
