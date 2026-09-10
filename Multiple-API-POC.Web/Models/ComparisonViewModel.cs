namespace Multiple_API_POC.Web.Models;

public class ComparisonViewModel
{
    public DemoViewModel Sequential { get; set; } = new();

    public DemoViewModel Parallel { get; set; } = new();

    public DemoViewModel CachedFirstRun { get; set; } = new();

    public DemoViewModel CachedSecondRun { get; set; } = new();
}