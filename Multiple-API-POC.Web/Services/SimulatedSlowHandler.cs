using System.Net;

namespace Multiple_API_POC.Web.Services;

public class SimulatedSlowHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await Task.Delay(
            TimeSpan.FromSeconds(5),
            cancellationToken);

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "Simulated slow service eventually responded.")
        };
    }
}