using System.Collections.Concurrent;
using System.Net;

namespace Multiple_API_POC.Web.Services;

public class SimulatedTransientFailureHandler : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, int> _attempts = new();

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string scenarioId =
            request.Headers.GetValues("X-Demo-Scenario-Id").First();

        int attempt = _attempts.AddOrUpdate(
            scenarioId,
            1,
            (_, current) => current + 1);

        HttpResponseMessage response;

        if (attempt <= 2)
        {
            response = new HttpResponseMessage(
                HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent(
                    "Simulated temporary failure")
            };
        }
        else
        {
            response = new HttpResponseMessage(
                HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "Simulated service recovered successfully")
            };

            _attempts.TryRemove(scenarioId, out _);
        }

        response.Headers.Add(
            "X-Demo-Attempt",
            attempt.ToString());

        return Task.FromResult(response);
    }
}