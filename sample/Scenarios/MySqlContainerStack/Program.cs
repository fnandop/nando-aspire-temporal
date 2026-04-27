// Scenario: Full Container Stack with MySQL
// Production-like setup with Temporal server backed by MySQL,
// plus Admin Tools and UI containers.

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Add MySQL for Temporal persistence
var mysql = builder.AddMySql("mysql")
    .WithLifetime(ContainerLifetime.Persistent);

// Add Temporal with full container stack
var temporal = builder.AddTemporal("temporal")
    .WithMySql(mysql)                // Use MySQL for persistence
    .WithtTemporalAdminTools()       // Add admin CLI tools container
    .WithtTemporalUi();              // Add web UI container

// Wire up your services
var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

var api = builder.AddProject<Projects.SampleApi>("api")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
