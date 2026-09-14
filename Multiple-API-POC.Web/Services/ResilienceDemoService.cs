using System.Diagnostics;
using Polly.CircuitBreaker;
using Polly.Timeout;

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

    public async Task<string> RunTimeoutTestAsync(
    CancellationToken cancellationToken = default)
    {
        HttpClient client =
            _httpClientFactory.CreateClient("TimeoutDemo");

        Stopwatch timer = Stopwatch.StartNew();

        try
        {
            await client.GetAsync(
                "https://example.invalid/slow-api",
                cancellationToken);

            timer.Stop();

            return $"Service responded after {timer.ElapsedMilliseconds} ms.";
        }
        catch (TimeoutRejectedException)
        {
            timer.Stop();

            return $"Request timed out after approximately " +
                   $"{timer.ElapsedMilliseconds} ms. " +
                   $"The portal stopped waiting for the slow service.";
        }
    }

    public async Task<List<string>> RunCircuitBreakerTestAsync(
        CancellationToken cancellationToken = default)
    {
        HttpClient client =
            _httpClientFactory.CreateClient("CircuitBreakerDemo");

        List<string> results = [];

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using HttpResponseMessage response =
                    await client.GetAsync(
                        "https://example.invalid/circuit-api",
                        cancellationToken);

                results.Add(
                    $"Request {attempt}: " +
                    $"{(int)response.StatusCode} {response.StatusCode}");
            }
            catch (BrokenCircuitException)
            {
                results.Add(
                    $"Request {attempt}: Circuit OPEN - " +
                    $"request was blocked immediately.");
            }
        }

        return results;
    }

    public async Task<List<string>> RunPartialSuccessTestAsync(
        CancellationToken cancellationToken = default)
    {
        HttpClient normalClient =
            _httpClientFactory.CreateClient("PartialSuccessDemo");

        HttpClient failingClient =
            _httpClientFactory.CreateClient("PartialFailureDemo");

        Task<string> weatherTask = CheckServiceAsync(
            normalClient,
            "Weather API",
            "https://api.open-meteo.com/v1/forecast?latitude=45.4215&longitude=-75.6972&current=temperature_2m",
            cancellationToken);

        Task<string> holidayTask = CheckServiceAsync(
            normalClient,
            "Canadian Public Holiday API",
            $"https://date.nager.at/api/v3/PublicHolidays/{DateTime.UtcNow.Year}/CA",
            cancellationToken);

        Task<string> dogTask = CheckServiceAsync(
            normalClient,
            "Dog API",
            "https://dog.ceo/api/breeds/image/random",
            cancellationToken);

        Task<string> failingTask = CheckServiceAsync(
            failingClient,
            "Simulated Unavailable API",
            "https://example.invalid/unavailable-api",
            cancellationToken);

        string[] results = await Task.WhenAll(
            weatherTask,
            holidayTask,
            dogTask,
            failingTask);

        return results.ToList();
    }

    private static async Task<string> CheckServiceAsync(
        HttpClient client,
        string name,
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response =
                await client.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return $"SUCCESS - {name}";
            }

            return $"FAILED - {name}: " +
                   $"{(int)response.StatusCode} {response.StatusCode}";
        }
        catch (Exception ex)
        {
            return $"FAILED - {name}: {ex.GetType().Name}";
        }
    }
}