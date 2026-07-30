using System.Security.Claims;
using System.Text.Json;
using FF.Architecture.Parsers;
using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FF_Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FavoriteApiController(FF_DbContext context) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> GetAll()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var favorites = await context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.SourceItem)
            .OrderByDescending(f => f.LastFavoriteAt)
            .ToListAsync();

        var news = favorites
            .Where(f => !string.IsNullOrWhiteSpace(f.SourceItem.Json))
            .Select(f => new { f.SourceItemId, News = JsonSerializer.Deserialize<NewsItemDto>(f.SourceItem.Json!, JsonOptions) })
            .Where(x => x.News is not null)
            .Select(x =>
            {
                x.News!.SourceItemId = x.SourceItemId;
                return x.News;
            })
            .ToList();

        return Ok(news);
    }

    [HttpPost("{sourceItemId:int}")]
    public async Task<IActionResult> Add(int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        if (!await context.SourceItems.AnyAsync(i => i.Id == sourceItemId)) return NotFound("La noticia no existe.");

        if (await context.Favorites.AnyAsync(f => f.UserId == userId && f.SourceItemId == sourceItemId))
            return Ok("La noticia ya era favorita.");

        context.Favorites.Add(new Favorite { UserId = userId.Value, SourceItemId = sourceItemId });
        await context.SaveChangesAsync();
        return Ok("Noticia agregada a favoritos.");
    }

    [HttpPut("{sourceItemId:int}/like")]
    public async Task<IActionResult> Like(int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var favorite = await context.Favorites.SingleOrDefaultAsync(f => f.UserId == userId && f.SourceItemId == sourceItemId);
        if (favorite is null) return NotFound("Primero agregue la noticia a favoritos.");

        favorite.LastFavoriteAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Ok("Like registrado; la noticia subió al inicio.");
    }

    [HttpDelete("{sourceItemId:int}")]
    public async Task<IActionResult> Remove(int sourceItemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var favorite = await context.Favorites.SingleOrDefaultAsync(f => f.UserId == userId && f.SourceItemId == sourceItemId);
        if (favorite is null) return NotFound();

        context.Favorites.Remove(favorite);
        await context.SaveChangesAsync();
        return NoContent();
    }

    private int? GetUserId() => int.TryParse(User.FindFirstValue("id"), out var id) ? id : null;
}

