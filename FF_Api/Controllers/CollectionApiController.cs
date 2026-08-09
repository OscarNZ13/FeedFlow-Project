using System.Security.Claims;
using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FF.Architecture.Parsers;

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

    [HttpGet("{id}/items")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> GetItems(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = await context.Collections
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (collection is null) return NotFound();

        var items = await context.CollectionItems
            .Where(ci => ci.CollectionId == id)
            .Include(ci => ci.SourceItem)
            .ToListAsync();

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var news = items
            .Where(ci => !string.IsNullOrWhiteSpace(ci.SourceItem.Json))
            .Select(ci => new { ci.SourceItemId, News = JsonSerializer.Deserialize<NewsItemDto>(ci.SourceItem.Json!, jsonOptions) })
            .Where(x => x.News is not null)
            .Select(x =>
            {
                x.News!.SourceItemId = x.SourceItemId;
                return x.News;
            })
            .ToList();

        return Ok(news);
    }

    [HttpPost("{id}/removeItem")]
    public async Task<IActionResult> RemoveItem(int id, [FromForm] int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = await context.Collections
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (collection is null) return NotFound();

        var item = await context.CollectionItems
            .SingleOrDefaultAsync(ci => ci.CollectionId == id && ci.SourceItemId == sourceItemId);
        if (item is null) return NotFound();

        context.CollectionItems.Remove(item);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private int? GetUserId() => int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
}
