using FF.Architecture.Parsers;
using FF_Api.Business;
using FF_Api.ViewModels;
using FF_ModelsDB.Models;
using FF_Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FF_Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FeedApiController(
    IFeedBusiness feedBusiness,
    ISourceRepository sourceRepository,
    ISourceItemRepository sourceItemRepository,
    ISourceSecretRepository sourceSecretRepository) : ControllerBase
{

    [HttpGet("feed")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> GetFeed([FromQuery] int take = 15)
    {
        var items = await feedBusiness.GetFeedAsync(take);
        return Ok(items);
    }


    [HttpGet("sources")]
    public async Task<ActionResult<IEnumerable<SourceViewModel>>> GetSources()
    {
        var sources = await sourceRepository.ReadAsync();
        return Ok(sources.Select(ToViewModel));
    }

    [HttpPost("sources")]
    public async Task<ActionResult<SourceViewModel>> CreateSource([FromBody] CreateSourceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Url y Name son obligatorios.");

        var hasSecret = !string.IsNullOrWhiteSpace(request.SecretKeyName);

        var source = new Source
        {
            Url = request.Url,
            Name = request.Name,
            Description = request.Description,
            ComponentType = string.IsNullOrWhiteSpace(request.ComponentType) ? "feed" : request.ComponentType,
            RequiresSecret = hasSecret,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await sourceRepository.CreateAsync(source);
        if (!created) return StatusCode(500, "No se pudo crear la fuente.");

        if (hasSecret)
        {
            await sourceSecretRepository.CreateAsync(new SourceSecret
            {
                SourceId = source.Id,
                KeyName = request.SecretKeyName!,
                KeyValue = request.SecretKeyValue ?? string.Empty,
                Location = request.SecretLocation
            });
        }

        return Ok(ToViewModel(source));
    }

    [HttpGet("sources/{id:int}/preview")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> PreviewSource(int id, [FromQuery] int take = 50)
    {
        try
        {
            var items = await feedBusiness.PreviewSourceAsync(id, take);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ApplicationException ex)
        {
            return StatusCode(502, ex.Message);
        }
    }

    [HttpPost("sources/{id:int}/refresh")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> RefreshSource(int id)
    {
        try
        {
            var items = await feedBusiness.RefreshSourceAsync(id);
            return Ok(items);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ApplicationException ex)
        {
            return StatusCode(502, ex.Message);
        }
    }

    // DELETE
    [HttpDelete("sources/{id:int}")]
    public async Task<IActionResult> DeleteSource(int id)
    {
        var source = await sourceRepository.FindAsync(id);
        if (source is null) return NotFound();

        try
        {
            var items = await sourceItemRepository.FindBySourceIdAsync(id);
            foreach (var item in items)
            {
                await sourceItemRepository.DeleteAsync(item);
            }

            var secrets = await sourceSecretRepository.ReadBySourceAsync(id);
            foreach (var secret in secrets)
            {
                await sourceSecretRepository.DeleteAsync(secret);
            }

            var deleted = await sourceRepository.DeleteAsync(source);
            if (!deleted) return StatusCode(500, "No se pudo eliminar la fuente (SaveChanges devolvió 0 filas).");

            return NoContent();
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, $"Error eliminando la fuente: {detail}");
        }
    }

    private static SourceViewModel ToViewModel(Source source) => new()
    {
        Id = source.Id,
        Url = source.Url,
        Name = source.Name,
        Description = source.Description,
        ComponentType = source.ComponentType,
        RequiresSecret = source.RequiresSecret,
        IsActive = source.IsActive,
        LastFetchedAt = source.LastFetchedAt
    };
}
