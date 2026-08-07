using FF.Architecture.Parsers;
using FF_DataDB.Context;
using FF_ModelsDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FF_Mvc.Controllers;

[Authorize]
public class CollectionController(IHttpClientFactory httpClientFactory) : Controller
{
    private const string ApiBaseUrl = "https://localhost:7283/CollectionApi";

    public async Task<IActionResult> Index()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync("https://localhost:7283/CollectionApi");
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "No fue posible cargar las colecciones.";
            return View(Enumerable.Empty<Collection>());
        }

        var collections = JsonSerializer.Deserialize<IEnumerable<Collection>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        return View(collections);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCollection(string name)
    {
        var client = CreateAuthenticatedClient();
        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("name", name) });
        var response = await client.PostAsync($"{ApiBaseUrl}/create", content);

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Colección creada."
            : "No fue posible crear la colección.";

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> RenameCollection(int id)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{ApiBaseUrl}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var collections = JsonSerializer.Deserialize<IEnumerable<Collection>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        var collection = collections.FirstOrDefault(c => c.Id == id);
        if (collection == null) return NotFound();

        return View(collection); // busca Views/Collection/RenameCollection.cshtml
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameConfirmed(int id, string newName)
    {
        var client = CreateAuthenticatedClient();
        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("newName", newName) });
        var response = await client.PutAsync($"{ApiBaseUrl}/{id}/rename", content);

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Colección renombrada."
            : "No fue posible renombrar la colección.";

        return RedirectToAction(nameof(Index));
    }



    [HttpGet]
    public async Task<IActionResult> DeleteCollection(int id)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"{ApiBaseUrl}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var collections = JsonSerializer.Deserialize<IEnumerable<Collection>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        var collection = collections.FirstOrDefault(c => c.Id == id);
        if (collection == null) return NotFound();

        return View(collection); // busca Views/Collection/DeleteCollection.cshtml
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.DeleteAsync($"{ApiBaseUrl}/{id}");

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Colección eliminada."
            : "No fue posible eliminar la colección.";

        return RedirectToAction(nameof(Index));
    }



    /*
    [HttpPost]
    public async Task<IActionResult> CreateCollection(int userId, string name)
    {
        var client = CreateAuthenticatedClient();
        var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("name", name) });
        var response = await client.PostAsync($"{ApiBaseUrl}/{userId}/create", content);
        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Colección creada."
            : "No fue posible crear la colección.";
        return RedirectToAction(nameof(Index), new { userId });
    }*/

    [HttpGet]
    public async Task<IActionResult> ViewCollection(int id)
    {
        var client = CreateAuthenticatedClient();
        var response = await client.GetAsync($"https://localhost:7283/CollectionApi/{id}/items");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "No fue posible cargar la colección.";
            return RedirectToAction(nameof(Index));
        }

        var items = JsonSerializer.Deserialize<IEnumerable<NewsItemDto>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        ViewBag.CollectionId = id;
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int collectionId, int sourceItemId)
    {
        var client = CreateAuthenticatedClient();
        var content = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string,string>("sourceItemId", sourceItemId.ToString())
    });

        var response = await client.PostAsync($"https://localhost:7283/CollectionApi/{collectionId}/removeItem", content);

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
            ? "Noticia eliminada de la colección."
            : "No fue posible eliminar la noticia.";

        return RedirectToAction(nameof(ViewCollection), new { id = collectionId });
    }



    private HttpClient CreateAuthenticatedClient()
    {
        var client = httpClientFactory.CreateClient();
        var token = HttpContext.Session.GetString("JwtToken");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
