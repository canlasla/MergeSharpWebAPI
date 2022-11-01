using MergeSharp;
using MergeSharpWebAPI.Models;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
    public ActionResult<string> GetAll() => JsonConvert.SerializeObject(LWWSetService<int>.GetAll());

    // Get LWW Set by id
    [HttpGet("GetLWWSet/{id}")]
    public ActionResult<string> Get(int id)
    {
        var lwwSet = LWWSetService<int>.Get(id);

        if (lwwSet == null)
            return NotFound();

        return JsonConvert.SerializeObject(lwwSet);
    }

    // Create LWW Set
    [HttpPost("CreateLWWSet")]
    public IActionResult Create(MergeSharpWebAPI.Models.LWWSet<int> lwwSet)
    {
        LWWSetService<int>.Add(lwwSet);
        return CreatedAtAction(nameof(Create), new { id = lwwSet.Id }, lwwSet);
    }

    // Update an LWW Set
    [HttpPut("UpdateLWWSet/{id}")]
    public IActionResult Update(int id, MergeSharpWebAPI.Models.LWWSet<int> lwwSet)
    {
        if (id != lwwSet.Id)
            return BadRequest();

        var existingLWWSet = LWWSetService<int>.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        LWWSetService<int>.Update(lwwSet);

        return NoContent();
    }

    // Delete an LWW Set
    [HttpDelete("DeleteLWWSet/{id}")]
    public IActionResult Delete(int id)
    {
        var lwwSet = LWWSetService<int>.Get(id);

        if (lwwSet is null)
            return NotFound();

        LWWSetService<int>.Delete(id);

        return NoContent();
    }

    // Add an element to LWW Set
    [HttpPost("AddElement/{id}")]
    public IActionResult AddElement(int id, int element)
    {
        var existingLWWSet = LWWSetService<int>.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        LWWSetService<int>.AddElement(id, element);

        return NoContent();
    }
    // Remove an element from LWW Set
    // Clear an LWW Set

}
