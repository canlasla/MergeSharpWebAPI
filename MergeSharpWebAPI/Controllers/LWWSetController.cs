using MergeSharpWebAPI.Models;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MergeSharpWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class LWWSetController<T> : ControllerBase
{
    public LWWSetController()
    {
    }

    [HttpGet]
    public ActionResult<List<LWWSet<T>>> GetAll() => LWWSetService<T>.GetAll();

    [HttpGet("{id}")]
    public ActionResult<LWWSet<T>> Get(int id)
    {
        var lwwSet = LWWSetService<T>.Get(id);

        if (lwwSet == null)
            return NotFound();

        return lwwSet;
    }

    [HttpPost]
    public IActionResult Create(LWWSet<T> lwwSet)
    {
        LWWSetService<T>.Add(lwwSet);
        return CreatedAtAction(nameof(Create), new { id = lwwSet.Id }, lwwSet);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, LWWSet<T> lwwSet)
    {
        if (id != lwwSet.Id)
            return BadRequest();

        var existingLWWSet = LWWSetService<T>.Get(id);
        if (existingLWWSet is null)
            return NotFound();

        LWWSetService<T>.Update(lwwSet);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var lwwSet = LWWSetService<T>.Get(id);

        if (lwwSet is null)
            return NotFound();

        LWWSetService<T>.Delete(id);

        return NoContent();
    }
}
