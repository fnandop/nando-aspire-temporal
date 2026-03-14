// Scenario: Temporal Dev Server
// The simplest setup - a lightweight in-memory Temporal server for local development.
// Includes a built-in UI at port 8233.

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Add the Temporal dev server (in-memory, no external dependencies)
var temporal = builder.AddTemporalDevServer("temporal");

// Wire up your worker and API projects
var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

var api = builder.AddProject<Projects.SampleApi>("api")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
