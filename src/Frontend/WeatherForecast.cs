namespace Frontend;

public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string Summary { get; set; } = string.Empty;
        
    public string Source { get; set; } = string.Empty;
}

public class Issue
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
}

public class WeatherClient
{
    private readonly HttpClient client;

    public WeatherClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<WeatherForecast[]?> GetWeatherAsync()
    {
        return await this.client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
    }

    public async Task<Issue[]?> GetIssuesAsync()
    {
        return await this.client.GetFromJsonAsync<Issue[]>("/issue");
    }

    public async Task PostIssueAsync(Issue issue)
    {
        BusinessWellnessHostService.NumberOfIssuesCreated.Add(1);
        var result = await this.client.PostAsJsonAsync("/issue", issue);
        result.EnsureSuccessStatusCode();
    }
}