# Nando.Aspire.Temporal

.NET Aspire hosting integration for **Temporal** — supports both local container-based development and Temporal Cloud.

- Package: `Nando.Aspire.Temporal`
- Targets: .NET 10

## Features

### Container-based Temporal (local development)

- Add Temporal as an Aspire container resource via `AddTemporal(...)`
- Lightweight dev server via `AddTemporalDevServer(...)`
- Optionally configure Temporal to use an Aspire Postgres resource via `WithPostgres(...)`
- Optional companion containers:
  - Temporal Admin Tools via `WithtTemporalAdminTools()`
  - Temporal UI via `WithtTemporalUi()`

### Temporal Cloud

- Connect to Temporal Cloud via `AddTemporalCloud(...)` with parameterized configuration
- Automatic environment variable injection for `TEMPORAL_ADDRESS`, `TEMPORAL_NAMESPACE`, and `TEMPORAL_API_KEY`

### Common

- Unified `ITemporalResource` interface for both container and cloud resources
- `WithReference(...)` extension to wire up projects to any Temporal resource

## Prerequisites

- .NET SDK 10.x
- Docker (required for container-based Temporal)
- .NET Aspire workload

## Installation

```bash
dotnet add package Nando.Aspire.Temporal
```

## Usage

### Temporal Dev Server (simplest local setup)

```csharp
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var temporal = builder.AddTemporalDevServer("temporal");

builder.AddProject<Projects.MyWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
```

### Temporal with Postgres + Admin Tools + UI

```csharp
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(5432)
    .WithPgAdmin();

var temporal = builder.AddTemporal("temporal")
    .WithPostgres(postgres)
    .WithtTemporalAdminTools()
    .WithtTemporalUi();

builder.AddProject<Projects.MyWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
```

### Temporal Cloud

```csharp
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var serverEndpoint = builder.AddParameter("temporal-server-endpoint");
var domain = builder.AddParameter("temporal-domain");
var apiKey = builder.AddParameter("temporal-api-key", secret: true);

var temporalCloud = builder.AddTemporalCloud("temporal-cloud", serverEndpoint, domain, apiKey);

builder.AddProject<Projects.MyWorker>("worker")
    .WithReference(temporalCloud);

builder.Build().Run();
```

Configure the parameters in `appsettings.json`:

```json
{
  "Parameters": {
    "temporal-server-endpoint": "your-namespace.tmprl.cloud:7233",
    "temporal-domain": "your-namespace",
    "temporal-api-key": "your-api-key"
  }
}
```

## Configuration

### Environment Variables (container images)

| Variable | Default | Description |
|----------|---------|-------------|
| `TEMPORAL_VERSION` | `latest` | Tag for `temporalio/auto-setup` and `temporalio/temporal` |
| `TEMPORAL_ADMINTOOLS_VERSION` | `latest` | Tag for `temporalio/admin-tools` |
| `TEMPORAL_UI_VERSION` | `latest` | Tag for `temporalio/ui` |

### Default Ports

| Service | Port |
|---------|------|
| Temporal gRPC | `7233` |
| Temporal UI | `8233` |

## API Reference

### Container Resources

| Method | Description |
|--------|-------------|
| `AddTemporal(builder, name, grpcPort?, domain?)` | Adds Temporal using `temporalio/auto-setup` image |
| `AddTemporalDevServer(builder, name, grpcPort?, uiPort?, domain?)` | Adds lightweight dev server using `temporalio/temporal` |
| `WithPostgres(temporal, postgres)` | Configures Temporal to use a Postgres resource |
| `WithtTemporalAdminTools(temporal)` | Adds the Temporal Admin Tools container |
| `WithtTemporalUi(temporal, uiPort?)` | Adds the Temporal UI container |

### Cloud Resources

| Method | Description |
|--------|-------------|
| `AddTemporalCloud(builder, name, serverEndpoint, domain, apiKey)` | Connects to Temporal Cloud using parameter resources |

### Common

| Method | Description |
|--------|-------------|
| `WithReference(builder, temporal)` | Injects Temporal connection info as environment variables |

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

## Release

This repo includes a GitHub Actions workflow to publish to NuGet on tagged releases (`v*.*.*`). See `.github/workflows/publish-nuget.yml`.

## License

See `LICENSE` (if present in this repository).
