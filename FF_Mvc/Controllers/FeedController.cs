using FF_Mvc.Service;
using Microsoft.AspNetCore.Mvc;

namespace FF_Mvc.Controllers;

public class FeedController(IFeedService feedService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = (await feedService.GetFeedAsync(50)).ToList();
        ViewBag.Total = items.Count;

        return View(items);
    }

    public IActionResult Download(int id)
    {
        return Redirect($"https://localhost:0/ImportExportApi/export/item/{id}");
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Seleccione un archivo JSON.";

            return View();
        }

        if (!string.Equals(Path.GetExtension(file.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Solo se permiten archivos JSON.";
            return RedirectToAction(nameof(Index));
        }

        using var reader = new StreamReader(file.OpenReadStream());

        string json = await reader.ReadToEndAsync();

        var result = await feedService.ImportItemAsync(json);

        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = result.Message;
        return View();
    }
}
