// Scenario: Temporal Dev Server with Custom Ports
// Dev server with custom gRPC and UI ports, plus a custom namespace (domain).

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Add dev server with custom configuration
var temporal = builder.AddTemporalDevServer(
    name: "temporal",
    grpcPort: 17233,      // Custom gRPC port
    uiPort: 18233,        // Custom UI port
    domain: "my-namespace" // Custom namespace
);

var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
