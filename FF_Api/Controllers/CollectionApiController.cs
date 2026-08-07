using FF.Architecture.Parsers;
using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

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

    /*
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
    }*/

    [HttpPost("{id}/addItem")]
    public async Task<IActionResult> AddItem(int id, [FromForm] int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var collection = await context.Collections
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (collection is null) return NotFound("Colección no encontrada.");

        if (!await context.SourceItems.AnyAsync(s => s.Id == sourceItemId))
            return NotFound("La noticia no existe.");

        if (await context.CollectionItems.AnyAsync(ci => ci.CollectionId == id && ci.SourceItemId == sourceItemId))
            return Ok("La noticia ya está en la colección.");

        context.CollectionItems.Add(new CollectionItem
        {
            CollectionId = id,
            SourceItemId = sourceItemId
        });

        await context.SaveChangesAsync();
        return Ok("Noticia agregada a la colección.");
    }


    [HttpGet("{id}/items")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> GetItems(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        // Validar que la colección existe y pertenece al usuario
        var collectionExists = await context.Collections.AnyAsync(c => c.Id == id && c.UserId == userId);
        if (!collectionExists) return NotFound("Colección no encontrada.");

        // Buscar los items de esa colección
        var items = await context.CollectionItems
            .Where(ci => ci.CollectionId == id)
            .Include(ci => ci.SourceItem)
            .ToListAsync();

        // Convertir cada SourceItem.Json en NewsItemDto
        var news = items
            .Where(ci => !string.IsNullOrWhiteSpace(ci.SourceItem.Json))
            .Select(ci =>
            {
                var dto = JsonSerializer.Deserialize<NewsItemDto>(ci.SourceItem.Json!,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto != null) dto.SourceItemId = ci.SourceItemId;
                return dto;
            })
            .Where(dto => dto != null)
            .ToList();

        return Ok(news);
    }


    [HttpPost("{id}/removeItem")]
    public async Task<IActionResult> RemoveItem(int id, [FromForm] int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        // Validar que la colección pertenece al usuario
        var collectionExists = await context.Collections.AnyAsync(c => c.Id == id && c.UserId == userId);
        if (!collectionExists) return NotFound("Colección no encontrada.");

        var item = await context.CollectionItems
            .SingleOrDefaultAsync(ci => ci.CollectionId == id && ci.SourceItemId == sourceItemId);

        if (item is null) return NotFound("La noticia no está en la colección.");

        context.CollectionItems.Remove(item);
        await context.SaveChangesAsync();
        return Ok("Noticia eliminada de la colección.");
    }




    private int? GetUserId() => int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
}
