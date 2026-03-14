// Scenario: Minimal Worker-Only Setup
// Just a Temporal dev server and a single worker - no API, no extras.
// Perfect for workflow development and testing.

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var temporal = builder.AddTemporalDevServer("temporal");

builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
