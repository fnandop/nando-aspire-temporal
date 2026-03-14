// Scenario: Environment-based Configuration
// Switch between dev server and Temporal Cloud based on environment.

using Microsoft.Extensions.Hosting;
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

IResourceBuilder<ITemporalResource> temporal;

if (isDevelopment)
{
    // Use lightweight dev server for local development
    temporal = builder.AddTemporalDevServer("temporal");
}
else
{
    // Use Temporal Cloud for staging/production
    var serverEndpoint = builder.AddParameter("temporal-server-endpoint");
    var domain = builder.AddParameter("temporal-domain");
    var apiKey = builder.AddParameter("temporal-api-key", secret: true);

    temporal = builder.AddTemporalCloud("temporal", serverEndpoint, domain, apiKey);
}

// Wire up services - works with both container and cloud resources
var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal);

var api = builder.AddProject<Projects.SampleApi>("api")
    .WithReference(temporal);

builder.Build().Run();
