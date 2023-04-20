using Common.Observability;
using Frontend;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient<WeatherClient>(client => {
    var targetBackendName = builder.Configuration["TargetBackendName"] ?? "BasicWebApi";
    var backendUri = builder.Configuration.GetServiceUri(targetBackendName) ?? new Uri("http://localhost:5027");
    client.BaseAddress = backendUri;
});

builder.AddObservability(meterProviderBuilder => meterProviderBuilder.AddMeter(BusinessWellnessHostService.MeterName));
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

app.MapBlazorHub();
app.MapRazorPages();

app.MapGet("/env", () =>
{
    var environmentVariables = Environment.GetEnvironmentVariables();
    var serialized = System.Text.Json.JsonSerializer.Serialize(environmentVariables);
    return serialized;
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapFallbackToPage("/_Host");
app.Run();