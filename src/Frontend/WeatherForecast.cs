using System;
using System.Text.Json;

namespace Frontend
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; } = string.Empty;
        
        public string Source { get; set; } = string.Empty;
    }

    public class WeatherClient
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly HttpClient client;

        public WeatherClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task<WeatherForecast[]?> GetWeatherAsync()
        {
            return await this.client.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");
        }
    }
}