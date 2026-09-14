using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Http.Resilience;
using Multiple_API_POC.Web.Services;
using Polly;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromSeconds(60),
        LocalCacheExpiration = TimeSpan.FromSeconds(60)
    };
});

builder.Services.AddHttpClient<DemoApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("ResilienceDemo")
    .ConfigurePrimaryHttpMessageHandler(() => new SimulatedFailureHandler())
    .AddStandardResilienceHandler();

builder.Services.AddScoped<ResilienceDemoService>();

builder.Services.AddHttpClient("TransientResilienceDemo")
    .ConfigurePrimaryHttpMessageHandler(
        () => new SimulatedTransientFailureHandler())
    .AddStandardResilienceHandler();

builder.Services.AddHttpClient("TimeoutDemo")
    .ConfigurePrimaryHttpMessageHandler(
        () => new SimulatedSlowHandler())
    .AddResilienceHandler("TimeoutPipeline", static pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(1));
    });

builder.Services.AddHttpClient("CircuitBreakerDemo")
    .ConfigurePrimaryHttpMessageHandler(
        () => new SimulatedFailureHandler())
    .AddResilienceHandler("CircuitBreakerPipeline", static pipeline =>
    {
        pipeline.AddCircuitBreaker(
            new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(10),
                BreakDuration = TimeSpan.FromSeconds(15),

                ShouldHandle = static args =>
                    ValueTask.FromResult(args is
                    {
                        Outcome.Result.StatusCode:
                            HttpStatusCode.ServiceUnavailable
                    })
            });
    });

builder.Services.AddHttpClient("PartialSuccessDemo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddHttpClient("PartialFailureDemo")
    .ConfigurePrimaryHttpMessageHandler(
        () => new SimulatedFailureHandler());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Demo}/{action=Index}/{id?}");

app.Run();
