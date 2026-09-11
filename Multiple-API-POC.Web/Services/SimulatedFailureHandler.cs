using System.Net;

namespace Multiple_API_POC.Web.Services;

public class SimulatedFailureHandler : HttpMessageHandler
{
    private int _attemptCount;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        int attempt = Interlocked.Increment(ref _attemptCount);

        HttpResponseMessage response = new(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent(
                "Simulated API failure: Service Unavailable")
        };

        response.Headers.Add(
            "X-Demo-Attempt",
            attempt.ToString());

        return Task.FromResult(response);
    }
}