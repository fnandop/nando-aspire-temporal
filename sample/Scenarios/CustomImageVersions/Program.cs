// Scenario: Custom Image Versions
// Pin specific versions of Temporal container images via configuration.
// Useful for reproducible builds and testing specific versions.

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

// Image versions are controlled via configuration:
// - TEMPORAL_VERSION: temporalio/auto-setup and temporalio/temporal
// - TEMPORAL_ADMINTOOLS_VERSION: temporalio/admin-tools  
// - TEMPORAL_UI_VERSION: temporalio/ui
//
// Set these in appsettings.json, environment variables, or user secrets.

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var temporal = builder.AddTemporal("temporal")
    .WithPostgres(postgres)
    .WithtTemporalAdminTools()
    .WithtTemporalUi();

var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
