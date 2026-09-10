using Microsoft.AspNetCore.Mvc;
using Multiple_API_POC.Web.Models;
using Multiple_API_POC.Web.Services;

namespace Multiple_API_POC.Web.Controllers;

public class DemoController : Controller
{
    private readonly DemoApiService _demoApiService;

    public DemoController(DemoApiService demoApiService)
    {
        _demoApiService = demoApiService;
    }

    public IActionResult Index()
    {
        DemoViewModel model = new()
        {
            Mode = "Ready"
        };

        return View(model);
    }

    public async Task<IActionResult> Sequential(
        CancellationToken cancellationToken)
    {
        DemoViewModel model =
            await _demoApiService.GetSequentialAsync(cancellationToken);

        return View("Index", model);
    }

    public async Task<IActionResult> Parallel(
        CancellationToken cancellationToken)
    {
        DemoViewModel model =
            await _demoApiService.GetParallelAsync(cancellationToken);

        return View("Index", model);
    }

    public async Task<IActionResult> Cached(
    CancellationToken cancellationToken)
    {
        DemoViewModel model =
            await _demoApiService.GetCachedAsync(cancellationToken);

        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache(
    CancellationToken cancellationToken)
    {
        await _demoApiService.ClearCacheAsync(cancellationToken);

        TempData["Message"] = "Cache cleared.";

        return RedirectToAction(nameof(Index));
    }
}