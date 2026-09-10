using System.Diagnostics;

using Multiple_API_POC.Web.Models;

namespace Multiple_API_POC.Web.Services;

public class DemoApiService
{
    private readonly HttpClient _httpClient;

    public DemoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DemoViewModel> GetSequentialAsync(
        CancellationToken cancellationToken = default)
    {
        Stopwatch totalTimer = Stopwatch.StartNew();

        int year = DateTime.UtcNow.Year;

        List<ApiResultModel> results = [];

        results.Add(await CallApiAsync(
            "Weather API",
            "https://api.open-meteo.com/v1/forecast?latitude=45.4215&longitude=-75.6972&current=temperature_2m",
            cancellationToken));

        results.Add(await CallApiAsync(
            "Canadian Public Holiday API",
            $"https://date.nager.at/api/v3/PublicHolidays/{year}/CA",
            cancellationToken));

        results.Add(await CallApiAsync(
            "Pokémon API",
            "https://pokeapi.co/api/v2/pokemon/pikachu",
            cancellationToken));

        results.Add(await CallApiAsync(
            "Dog API",
            "https://dog.ceo/api/breeds/image/random",
            cancellationToken));

        totalTimer.Stop();

        return new DemoViewModel
        {
            Mode = "Sequential",
            Results = results,
            TotalElapsedMilliseconds = totalTimer.ElapsedMilliseconds
        };
    }

    public async Task<DemoViewModel> GetParallelAsync(
        CancellationToken cancellationToken = default)
    {
        Stopwatch totalTimer = Stopwatch.StartNew();

        int year = DateTime.UtcNow.Year;

        Task<ApiResultModel> weatherTask = CallApiAsync(
            "Weather API",
            "https://api.open-meteo.com/v1/forecast?latitude=45.4215&longitude=-75.6972&current=temperature_2m",
            cancellationToken);

        Task<ApiResultModel> holidayTask = CallApiAsync(
            "Canadian Public Holiday API",
            $"https://date.nager.at/api/v3/PublicHolidays/{year}/CA",
            cancellationToken);

        Task<ApiResultModel> pokemonTask = CallApiAsync(
            "Pokémon API",
            "https://pokeapi.co/api/v2/pokemon/pikachu",
            cancellationToken);

        Task<ApiResultModel> dogTask = CallApiAsync(
            "Dog API",
            "https://dog.ceo/api/breeds/image/random",
            cancellationToken);

        ApiResultModel[] results = await Task.WhenAll(
            weatherTask,
            holidayTask,
            pokemonTask,
            dogTask);

        totalTimer.Stop();

        return new DemoViewModel
        {
            Mode = "Parallel",
            Results = results.ToList(),
            TotalElapsedMilliseconds = totalTimer.ElapsedMilliseconds
        };
    }

    private async Task<ApiResultModel> CallApiAsync(
        string name,
        string url,
        CancellationToken cancellationToken)
    {
        Stopwatch timer = Stopwatch.StartNew();

        try
        {
            string response = await _httpClient.GetStringAsync(
                url,
                cancellationToken);

            timer.Stop();

            if (response.Length > 250)
            {
                response = response[..250] + "...";
            }

            return new ApiResultModel
            {
                Name = name,
                Response = response,
                ElapsedMilliseconds = timer.ElapsedMilliseconds,
                Source = "API"
            };
        }
        catch (Exception ex)
        {
            timer.Stop();

            return new ApiResultModel
            {
                Name = name,
                Response = $"Error: {ex.Message}",
                ElapsedMilliseconds = timer.ElapsedMilliseconds,
                Source = "API"
            };
        }
    }
}