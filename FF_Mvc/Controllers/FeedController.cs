using FF_Mvc.Service;
using Microsoft.AspNetCore.Mvc;

namespace FF_Mvc.Controllers;

public class FeedController(IFeedService feedService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await feedService.GetFeedAsync(10);
        return View(items);
    }
}
