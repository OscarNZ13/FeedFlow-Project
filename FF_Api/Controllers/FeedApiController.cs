using FF.Architecture.Parsers;
using FF_Api.Business;
using FF_Api.ViewModels;
using FF_ModelsDB;
using FF_Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FF_Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FeedApiController(
    IFeedBusiness feedBusiness,
    ISourceRepository sourceRepository,
    ISourceSecretRepository sourceSecretRepository) : ControllerBase
{
    // GET FeedApi/feed?take=10
    // Endpoint principal: lo que consume la Landing Page / feed de noticias.
    [HttpGet("feed")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> GetFeed([FromQuery] int take = 10)
    {
        var items = await feedBusiness.GetFeedAsync(take);
        return Ok(items);
    }

    // GET FeedApi/sources
    [HttpGet("sources")]
    public async Task<ActionResult<IEnumerable<SourceViewModel>>> GetSources()
    {
        var sources = await sourceRepository.ReadAsync();
        return Ok(sources.Select(ToViewModel));
    }

    // POST FeedApi/sources  -> lo que usa el formulario "agregar fuente" (solo Admin)
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

    // GET FeedApi/sources/5/preview?take=10  -> trae y parsea EN VIVO, sin guardar
    [HttpGet("sources/{id:int}/preview")]
    public async Task<ActionResult<IEnumerable<NewsItemDto>>> PreviewSource(int id, [FromQuery] int take = 10)
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
            return StatusCode(502, ex.Message); // fuente externa no respondió / formato inválido
        }
    }

    // POST FeedApi/sources/5/refresh  -> trae, parsea y GUARDA items nuevos en SourceItems
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
