using Marten;
using Marten.AspNetCore;
using Marten.Schema;
using Microsoft.AspNetCore.Mvc;

namespace BasicWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MartenSampleController : ControllerBase
{
    [HttpGet("/issue/{issueId}")]
    public Task Get(Guid issueId, [FromServices] IQuerySession session, [FromServices]ILogger<MartenSampleController> logger)
    {
        logger.LogInformation("Doing some informational logging to show on trace.");
        logger.LogInformation("Getting a specific Issue with ID: {IssueId}", issueId);
        // This "streams" the raw JSON to the HttpResponse
        // w/o ever having to read the full JSON string or
        // deserialize/serialize within the HTTP request
        return session.Json.WriteById<Issue>(issueId, HttpContext);
    }
    
    [HttpGet("/issue")]
    public Task Get([FromServices] IQuerySession session, [FromServices]ILogger<MartenSampleController> logger)
    {
        logger.LogInformation("Doing some informational logging to show on trace.");
        logger.LogInformation("Getting all Issues without ID filter.");

        return session
            .Query<Issue>()
            .Take(10)
            .WriteArray(HttpContext);
    }

    [HttpPost("/issue")]
    public async Task<ActionResult> Post([FromBody] Issue issue, [FromServices] IDocumentStore store, [FromServices]ILogger<MartenSampleController> logger)
    {
        if (!TryValidateModel(issue))
        {
            return ValidationProblem("The given Issue does not validate.");
        }

        logger.LogInformation("Doing some informational logging to show on trace.");
        logger.LogInformation("Posting Issue with ID: {IssueId}", issue.Id);

        await using var session = store.LightweightSession();
        session.Store(issue);
        await session.SaveChangesAsync();

        return Ok();
    }
}

public class Issue
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
}

public class InitialIssueData : IInitialData
{
    private readonly IEnumerable<Issue> _initialData = new List<Issue>
    {
        new() {Id = Guid.Parse("2219b6f7-7883-4629-95d5-1a8a6c74b244"), Title = "Sample Issue A"},
        new() {Id = Guid.Parse("642a3e95-5875-498e-8ca0-93639ddfebcd"), Title = "Sample Issue B"},
        new() {Id = Guid.Parse("331c15b4-b7bd-44d6-a804-b6879f99a65f"), Title = "Sample Issue C"},
    };

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        session.Store(_initialData);

        await session.SaveChangesAsync(cancellation);
    }
}