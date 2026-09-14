using Microsoft.AspNetCore.Mvc;
using Multiple_API_POC.Web.Models;
using Multiple_API_POC.Web.Services;

namespace Multiple_API_POC.Web.Controllers;

public class DemoController : Controller
{
    private readonly DemoApiService _demoApiService;
    private readonly ResilienceDemoService _resilienceDemoService;

    public DemoController(DemoApiService demoApiService, ResilienceDemoService resilienceDemoService)
    {
        _demoApiService = demoApiService;
        _resilienceDemoService = resilienceDemoService;
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

    public async Task<IActionResult> Comparison(
    CancellationToken cancellationToken)
    {
        ComparisonViewModel model =
            await _demoApiService.GetComparisonAsync(cancellationToken);

        return View(model);
    }

    public IActionResult Resilience()
    {
        return View();
    }

    public async Task<IActionResult> RunResilience(
    CancellationToken cancellationToken)
    {
        string result =
            await _resilienceDemoService.RunFailureTestAsync(cancellationToken);

        ViewBag.Result = result;

        return View("Resilience");
    }

    public async Task<IActionResult> RunTransientResilience(
    CancellationToken cancellationToken)
    {
        string result =
            await _resilienceDemoService.RunTransientFailureTestAsync(
                cancellationToken);

        ViewBag.TransientResult = result;

        return View("Resilience");
    }

    public async Task<IActionResult> RunTimeoutResilience(
    CancellationToken cancellationToken)
    {
        ViewBag.TimeoutResult =
            await _resilienceDemoService.RunTimeoutTestAsync(
                cancellationToken);

        return View("Resilience");
    }


    public async Task<IActionResult> RunCircuitBreakerResilience(
        CancellationToken cancellationToken)
    {
        ViewBag.CircuitResults =
            await _resilienceDemoService.RunCircuitBreakerTestAsync(
                cancellationToken);

        return View("Resilience");
    }


    public async Task<IActionResult> RunPartialSuccessResilience(
        CancellationToken cancellationToken)
    {
        ViewBag.PartialResults =
            await _resilienceDemoService.RunPartialSuccessTestAsync(
                cancellationToken);

        return View("Resilience");
    }
}