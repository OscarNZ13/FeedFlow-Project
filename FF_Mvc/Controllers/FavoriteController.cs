using System.Net.Http.Headers;
using System.Text.Json;
using FF.Architecture.Parsers;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FF_Mvc.Controllers;

[Authorize]
public class FavoriteController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string ApiBaseUrl = "https://localhost:7283/FavoriteApi";

    /*
    public async Task<IActionResult> Index()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync(ApiBaseUrl);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "No fue posible cargar los favoritos.";
            return View(Enumerable.Empty<NewsItemDto>());
        }

        var items = JsonSerializer.Deserialize<IEnumerable<NewsItemDto>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        return View(items);
    }
    */

    public async Task<IActionResult> Index()
    {
        var client = CreateAuthenticatedClient();

        // Favoritos
        var response = await client.GetAsync("https://localhost:7283/FavoriteApi");
        var items = response.IsSuccessStatusCode
            ? JsonSerializer.Deserialize<IEnumerable<NewsItemDto>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? []
            : Enumerable.Empty<NewsItemDto>();

        return View(items);
    }


    /*
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int sourceItemId)
    {
        var response = await CreateAuthenticatedClient().PostAsync($"{ApiBaseUrl}/{sourceItemId}", null);
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Noticia agregada a favoritos."
            : "No fue posible agregar la noticia a favoritos.";
        return RedirectToAction("Index", "Feed");
    }*/

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int sourceItemId, int[]? collectionIds)
    {
        var client = CreateAuthenticatedClient();

        // Siempre guardar en favoritos
        var response = await client.PostAsync($"{ApiBaseUrl}/{sourceItemId}", null);

        // Si el usuario seleccionó colecciones, guardar también en ellas
        if (response.IsSuccessStatusCode && collectionIds != null && collectionIds.Any())
        {
            foreach (var collectionId in collectionIds)
            {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string,string>("sourceItemId", sourceItemId.ToString())
            });
                await client.PostAsync($"https://localhost:7283/CollectionApi/{collectionId}/addItem", content);
            }
        }

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Noticia agregada a favoritos."
            : "No fue posible agregar la noticia a favoritos.";

        return RedirectToAction("Index", "Feed");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int sourceItemId)
    {
        var response = await CreateAuthenticatedClient().PutAsync($"{ApiBaseUrl}/{sourceItemId}/like", null);
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Like registrado. La noticia ahora aparece de primera."
            : "No fue posible registrar el like.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int sourceItemId)
    {
        var response = await CreateAuthenticatedClient().DeleteAsync($"{ApiBaseUrl}/{sourceItemId}");
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Noticia eliminada de favoritos."
            : "No fue posible eliminar la noticia.";
        return RedirectToAction(nameof(Index));
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = httpClientFactory.CreateClient();
        var token = HttpContext.Session.GetString("JwtToken");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

