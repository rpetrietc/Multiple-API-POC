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

        string attempts = response.Headers.TryGetValues(
        "X-Demo-Attempt",
        out IEnumerable<string>? values)
            ? values.First()
            : "Unknown";

        return $"Final result: {(int)response.StatusCode} {response.StatusCode} " +
               $"after {timer.ElapsedMilliseconds} ms. " +
               $"Total attempts: {attempts}";
    }

    public async Task<string> RunTransientFailureTestAsync(
    CancellationToken cancellationToken = default)
    {
        HttpClient client =
            _httpClientFactory.CreateClient("TransientResilienceDemo");

        using HttpRequestMessage request = new(
            HttpMethod.Get,
            "https://example.invalid/transient-api");

        request.Headers.Add(
            "X-Demo-Scenario-Id",
            Guid.NewGuid().ToString());

        Stopwatch timer = Stopwatch.StartNew();

        HttpResponseMessage response = await client.SendAsync(
            request,
            cancellationToken);

        timer.Stop();

        string attempts = response.Headers.TryGetValues(
            "X-Demo-Attempt",
            out IEnumerable<string>? values)
                ? values.First()
                : "Unknown";

        return $"Final result: {(int)response.StatusCode} {response.StatusCode} " +
               $"after {timer.ElapsedMilliseconds} ms. " +
               $"Total attempts: {attempts}";
    }
}