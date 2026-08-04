using FF_Mvc.Service;
using FF_Mvc.ViewModels;
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
        return Redirect($"https://localhost:7283/ImportExportApi/export/item/{id}");
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

    private bool IsAdmin()
    {
        var roleIdClaim = User.FindFirst("roleId");
        return roleIdClaim != null && roleIdClaim.Value == "1";
    }

    public async Task<IActionResult> Sources()
    {
        if (!IsAdmin())
        {
            TempData["Error"] = "Solo un administrador puede ver las fuentes.";
            return RedirectToAction(nameof(Index));
        }

        var sources = await feedService.GetSourcesAsync();
        return View(sources);
    }

    public IActionResult AddSource()
    {
        if (!IsAdmin())
        {
            TempData["Error"] = "Solo un administrador puede agregar fuentes.";
            return RedirectToAction(nameof(Index));
        }

        return View(new SourceFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSource(SourceFormViewModel form)
    {
        if (!IsAdmin())
        {
            TempData["Error"] = "Solo un administrador puede agregar fuentes.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
            return View(form);

        var created = await feedService.CreateSourceAsync(form);
        if (created is null)
        {
            ModelState.AddModelError(string.Empty, "No se pudo crear la fuente. Intenta de nuevo.");
            return View(form);
        }

        TempData["Success"] = $"Fuente \"{created.Name}\" agregada correctamente.";
        return RedirectToAction(nameof(Sources));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshSource(int id)
    {
        if (!IsAdmin())
        {
            TempData["Error"] = "Solo un administrador puede refrescar fuentes.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var items = await feedService.RefreshSourceAsync(id);
            TempData["Success"] = $"Se procesaron {items.Count()} items de la fuente.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"No se pudo refrescar la fuente: {ex.Message}";
        }

        return RedirectToAction(nameof(Sources));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSource(int id)
    {
        if (!IsAdmin())
        {
            TempData["Error"] = "Solo un administrador puede eliminar fuentes.";
            return RedirectToAction(nameof(Index));
        }

        var (success, error) = await feedService.DeleteSourceAsync(id);
        if (success)
        {
            TempData["Success"] = "Fuente eliminada correctamente.";
        }
        else
        {
            TempData["Error"] = $"No se pudo eliminar la fuente: {error}";
        }

        return RedirectToAction(nameof(Sources));
    }
}