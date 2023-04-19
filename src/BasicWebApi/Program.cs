using BasicWebApi.Controllers;
using Common.Observability;
using Frontend;
using Marten;
using Npgsql;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddObservability(
    meterProviderBuilder => meterProviderBuilder.AddMeter(BusinessWellnessHostService.MeterName),
    tracerProviderBuilder => tracerProviderBuilder.AddNpgsql());

builder.Services.AddHostedService<BusinessWellnessHostService>();

builder.Services
    .AddMarten(opts =>
    {
        opts.Connection(builder.Configuration.GetConnectionString("Marten"));
        opts.AutoCreateSchemaObjects = AutoCreate.All;
    })
    .InitializeWith<InitialIssueData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/env", () =>
{
    var environmentVariables = Environment.GetEnvironmentVariables();
    var serialized = System.Text.Json.JsonSerializer.Serialize(environmentVariables);
    return serialized;
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();
