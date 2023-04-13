using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public WeatherForecast[]? Forecasts { get; set; } = Array.Empty<WeatherForecast>();
    
    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGet([FromServices]WeatherClient client)
    {
        Forecasts = await client.GetWeatherAsync();
    }
}
