using System.Security.Claims;
using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FF_Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CollectionApiController(FF_DbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCollections()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collections = await context.Collections
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return Ok(collections);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCollection([FromForm] string name)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = new Collection { UserId = userId.Value, Name = name };
        context.Collections.Add(collection);
        await context.SaveChangesAsync();
        return Ok(collection);
    }

    [HttpPut("{id}/rename")]
    public async Task<IActionResult> RenameCollection(int id, [FromForm] string newName)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = await context.Collections.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (collection == null) return NotFound();

        collection.Name = newName;
        await context.SaveChangesAsync();
        return Ok(collection);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollection(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = await context.Collections.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (collection == null) return NotFound();

        context.Collections.Remove(collection);
        await context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPost("{id}/addItem")]
    public async Task<IActionResult> AddItem(int id, [FromForm] int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = await context.Collections.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (collection is null) return NotFound();

        var item = new CollectionItem { CollectionId = id, SourceItemId = sourceItemId };
        context.CollectionItems.Add(item);
        await context.SaveChangesAsync();
        return Ok(item);
    }

    private int? GetUserId() => int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
}
