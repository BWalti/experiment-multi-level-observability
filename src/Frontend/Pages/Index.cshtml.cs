using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public WeatherForecast[]? Forecasts { get; set; } = Array.Empty<WeatherForecast>();
    public Issue[]? Issues { get; set; } = Array.Empty<Issue>();

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGet([FromServices]WeatherClient client)
    {
        Forecasts = await client.GetWeatherAsync();
        await GetIssues(client);
    }

    public async Task GetIssues(WeatherClient client)
    {
        _logger.LogInformation("Getting issues from backend API...");
        Issues = await client.GetIssuesAsync();
    }
}
