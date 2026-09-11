using System.Net;

namespace Multiple_API_POC.Web.Services;

public class SimulatedFailureHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = new(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent(
                "Simulated API failure: Service Unavailable")
        };

        return Task.FromResult(response);
    }
}