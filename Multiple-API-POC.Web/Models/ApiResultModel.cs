namespace Multiple_API_POC.Web.Models;

public class ApiResultModel
{
    public string Name { get; set; } = string.Empty;

    public string Response { get; set; } = string.Empty;

    public long ElapsedMilliseconds { get; set; }

    public string Source { get; set; } = "API";
}