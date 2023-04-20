using System.Diagnostics.Metrics;

namespace Frontend;

public class BusinessWellnessHostService : IHostedService
{
    public const string MeterName = "Frontend.BusinessWellness";

    private readonly ILogger<BusinessWellnessHostService> _logger;
    private static readonly Meter MyMeter = new(MeterName, "1.0");

    /// <summary>
    /// The following gauge exposes some kind of "Business Wellness" statistics per domain business processes
    /// </summary>
    private static readonly ObservableGauge<int> BusinessWellness =
        MyMeter.CreateObservableGauge("business_wellness", () =>
        {
            return new Measurement<int>[]
            {
                new(WellnessState.GetRandom(), new KeyValuePair<string, object?>("kind", "api"))
            };
        });

    public BusinessWellnessHostService(ILogger<BusinessWellnessHostService> logger)
    {
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Business Wellness Metrics...");
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Business Wellness Metrics...");

        return Task.CompletedTask;
    }
}

internal static class WellnessState
{
    private const int Healthy = 0;
    private const int Degraded = 10;
    private const int Unhealthy = 20;

    public static int GetRandom() =>
        Random.Shared.Next(100) switch
        {
            // around 80% healthy, 15% degraded, 5% unhealthy
            >=0 and <80 => Healthy,
            >=80 and <95 => Degraded,
            >=95 and <100 => Unhealthy,
            _ => throw new ArgumentOutOfRangeException()
        };
}