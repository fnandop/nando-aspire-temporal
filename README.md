# Nando.Aspire.Temporal

Aspire hosting integration for running **Temporal** as a container resource from a .NET Aspire `AppHost`.

- Package: `Nando.Aspire.Temporal`
- Targets: .NET 10

## Features

- Add Temporal as an Aspire container resource via `AddTemporal(...)`
- Optionally configure Temporal to use an Aspire Postgres resource via `WithPostgres(...)`
- Optional companion containers:
  - Temporal Admin Tools via `WithtTemporalAdminTools()`
  - Temporal UI via `WithtTemporalUi()`
- Connection string support through `IResourceWithConnectionString`

## Prerequisites

- .NET SDK 10.x
- Docker (required to run the containers)
- .NET Aspire workload set up for your environment

## Installation

```bash
dotnet add package Nando.Aspire.Temporal
```

## Usage

### Minimal: Temporal

```csharp
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddTemporal("temporal");

builder.Build().Run();
```

### Temporal + Postgres + Admin Tools + UI (sample)

This matches `sample/AppHost/AppHost.AppHost/AppHost.cs`.

```csharp
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHostPort(5432)
    .WithPgAdmin();

builder.AddTemporal("temporal")
    .WithPostgres(postgres)
    .WithtTemporalAdminTools()
    .WithtTemporalUi();

builder.Build().Run();
```

## Configuration

The following environment variables can be used to control image tags at runtime:

- `TEMPORAL_VERSION` (default: `latest`)
- `TEMPORAL_ADMINTOOLS_VERSION` (default: `latest`)
- `TEMPORAL_UI_VERSION` (default: `latest`)

Ports (overridable via parameters):

- Temporal gRPC: `7233`
- Temporal UI: `8233`

## API

- `AddTemporal(builder, name, grpcPort: null)`
  - Uses image `temporalio/auto-setup:{TEMPORAL_VERSION}`
  - Exposes a `grpc` endpoint named `temporal-server`
- `WithPostgres(temporal, postgres)`
  - Configures Temporal to use the referenced Postgres resource
- `WithtTemporalAdminTools(temporal)`
  - Adds `temporalio/admin-tools:{TEMPORAL_ADMINTOOLS_VERSION}`
- `WithtTemporalUi(temporal, uiPort: null)`
  - Adds `temporalio/ui:{TEMPORAL_UI_VERSION}`
  - Exposes an `http` endpoint named `temporal-server-ui`
- `AddTemporalDevServer(builder, name, grpcPort: null, uiPort: null)`
  - Uses image `temporalio/temporal:{TEMPORAL_VERSION}` and runs `server start-dev`

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
