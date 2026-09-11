using System.Diagnostics;

namespace Multiple_API_POC.Web.Services;

public class ResilienceDemoService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResilienceDemoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> RunFailureTestAsync(
        CancellationToken cancellationToken = default)
    {
        HttpClient client =
            _httpClientFactory.CreateClient("ResilienceDemo");

        Stopwatch timer = Stopwatch.StartNew();

        HttpResponseMessage response = await client.GetAsync(
            "https://example.invalid/simulated-api",
            cancellationToken);

        timer.Stop();

        return $"Final result: {(int)response.StatusCode} {response.StatusCode} " +
               $"after {timer.ElapsedMilliseconds} ms";
    }
}