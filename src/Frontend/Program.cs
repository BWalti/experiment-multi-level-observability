using System.Reflection;
using Frontend;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(
        builder.Environment.ApplicationName,
        serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
        serviceInstanceId: Environment.MachineName);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<WeatherClient>(client => {
    var targetBackendName = builder.Configuration["TargetBackendName"] ?? "BasicWebApi";
    var backendUri = builder.Configuration.GetServiceUri(targetBackendName) ?? new Uri("http://localhost:5027");
    client.BaseAddress = backendUri;
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(b => b
        .SetResourceBuilder(resourceBuilder)
        .AddMeter("Frontend.BusinessWellness")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter()
        .AddOtlpExporter(c =>
        {
            var otlpTargetName = builder.Configuration["OtlpTargetName"] ?? string.Empty;
            var otlpTargetUri = builder.Configuration.GetServiceUri(otlpTargetName, "otlp-receiver");
            c.Endpoint = otlpTargetUri;

            c.BatchExportProcessorOptions.ScheduledDelayMilliseconds = 1000;
            c.TimeoutMilliseconds = 1000;
        })
    );

builder.Services.AddHostedService<BusinessWellnessHostService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapRazorPages();

app.MapGet("/env", () =>
{
    var environmentVariables = Environment.GetEnvironmentVariables();
    var serialized = System.Text.Json.JsonSerializer.Serialize(environmentVariables);
    return serialized;
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();