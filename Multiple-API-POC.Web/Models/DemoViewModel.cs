namespace Multiple_API_POC.Web.Models;

public class DemoViewModel
{
    public List<ApiResultModel> Results { get; set; } = [];

    public long TotalElapsedMilliseconds { get; set; }

    public string Mode { get; set; } = string.Empty;
}