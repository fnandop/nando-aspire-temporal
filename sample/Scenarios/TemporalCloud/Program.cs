// Scenario: Temporal Cloud
// Connect to Temporal Cloud for production workloads.
// Requires configuration in appsettings.json or user secrets.

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Define parameters for Temporal Cloud connection
var serverEndpoint = builder.AddParameter("temporal-server-endpoint");
var domain = builder.AddParameter("temporal-domain");
var apiKey = builder.AddParameter("temporal-api-key", secret: true);

// Add Temporal Cloud resource
var temporal = builder.AddTemporalCloud("temporal", serverEndpoint, domain, apiKey);

// Wire up your services
var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal);

var api = builder.AddProject<Projects.SampleApi>("api")
    .WithReference(temporal);

builder.Build().Run();
