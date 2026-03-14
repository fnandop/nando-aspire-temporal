// Scenario: Full Container Stack with Postgres
// Production-like setup with Temporal server backed by PostgreSQL,
// plus Admin Tools and UI containers.

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL for Temporal persistence
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(5432)
    .WithPgAdmin();

// Add Temporal with full container stack
var temporal = builder.AddTemporal("temporal")
    .WithPostgres(postgres)          // Use Postgres for persistence
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
