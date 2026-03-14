// Multi-Scenario AppHost
// Select a scenario via configuration: "Scenario" in appsettings.json, 
// environment variable, or command line argument.
//
// Supported scenarios:
//   - DevServer (default)
//   - DevServerCustomPorts
//   - FullContainerStack
//   - TemporalCloud
//   - MinimalWorkerOnly

using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var scenario = builder.Configuration["Scenario"] ?? "DevServer";

IResourceBuilder<ITemporalResource> temporal = scenario switch
{
    "DevServer" => ConfigureDevServer(builder),
    "DevServerCustomPorts" => ConfigureDevServerCustomPorts(builder),
    "FullContainerStack" => ConfigureFullContainerStack(builder),
    "TemporalCloud" => ConfigureTemporalCloud(builder),
    "MinimalWorkerOnly" => ConfigureDevServer(builder),
    _ => throw new InvalidOperationException($"Unknown scenario: {scenario}")
};

// Add services (skip API for MinimalWorkerOnly)
var worker = builder.AddProject<Projects.SampleWorker>("worker")
    .WithReference(temporal);

if (scenario != "MinimalWorkerOnly")
{
    var api = builder.AddProject<Projects.SampleApi>("api")
        .WithReference(temporal);
}

builder.Build().Run();

// --- Scenario Configurations ---

static IResourceBuilder<ContainerTemporalResource> ConfigureDevServer(IDistributedApplicationBuilder builder)
{
    return builder.AddTemporalDevServer("temporal");
}

static IResourceBuilder<ContainerTemporalResource> ConfigureDevServerCustomPorts(IDistributedApplicationBuilder builder)
{
    return builder.AddTemporalDevServer(
        name: "temporal",
        grpcPort: 17233,
        uiPort: 18233,
        domain: "custom-namespace"
    );
}

static IResourceBuilder<ContainerTemporalResource> ConfigureFullContainerStack(IDistributedApplicationBuilder builder)
{
    var postgres = builder.AddPostgres("postgres")
        .WithLifetime(ContainerLifetime.Persistent)
        .WithHostPort(5432)
        .WithPgAdmin();

    return builder.AddTemporal("temporal")
        .WithPostgres(postgres)
        .WithtTemporalAdminTools()
        .WithtTemporalUi();
}

static IResourceBuilder<CloudTemporalResource> ConfigureTemporalCloud(IDistributedApplicationBuilder builder)
{
    var serverEndpoint = builder.AddParameter("temporal-server-endpoint");
    var domain = builder.AddParameter("temporal-domain");
    var apiKey = builder.AddParameter("temporal-api-key", secret: true);

    return builder.AddTemporalCloud("temporal", serverEndpoint, domain, apiKey);
}
