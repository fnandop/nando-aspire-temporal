# Nando.Aspire.Temporal

.NET Aspire hosting integration for [Temporal](https://temporal.io/) — supports both local container-based development and Temporal Cloud.

[![NuGet](https://img.shields.io/nuget/v/Nando.Aspire.Temporal.svg)](https://www.nuget.org/packages/Nando.Aspire.Temporal)

## Installation

```bash
dotnet add package Nando.Aspire.Temporal
```

## Quick Start

### Dev Server (simplest setup)

```csharp
using Nando.Aspire.Temporal;

var builder = DistributedApplication.CreateBuilder(args);

var temporal = builder.AddTemporalDevServer("temporal");

builder.AddProject<Projects.MyWorker>("worker")
    .WithReference(temporal)
    .WaitFor(temporal);

builder.Build().Run();
```

### Temporal with Postgres

```csharp
var postgres = builder.AddPostgres("postgres");

var temporal = builder.AddTemporal("temporal")
    .WithPostgres(postgres)
    .WithtTemporalAdminTools()
    .WithtTemporalUi();
```

### Temporal Cloud

```csharp
var serverEndpoint = builder.AddParameter("temporal-server-endpoint");
var domain = builder.AddParameter("temporal-domain");
var apiKey = builder.AddParameter("temporal-api-key", secret: true);

var temporal = builder.AddTemporalCloud("temporal", serverEndpoint, domain, apiKey);

builder.AddProject<Projects.MyWorker>("worker")
    .WithReference(temporal);
```

## Features

| Feature | Method |
|---------|--------|
| Dev Server (in-memory) | `AddTemporalDevServer()` |
| Full Temporal Server | `AddTemporal()` |
| Postgres persistence | `WithPostgres()` |
| Admin Tools container | `WithtTemporalAdminTools()` |
| Web UI container | `WithtTemporalUi()` |
| Temporal Cloud | `AddTemporalCloud()` |

## Environment Variables

Injected automatically via `WithReference()`:

- `TEMPORAL_ADDRESS` — Server endpoint (e.g., `localhost:7233`)
- `TEMPORAL_NAMESPACE` — Namespace/domain
- `TEMPORAL_API_KEY` — API key (Temporal Cloud only)

## Configuration

Control container image versions via configuration:

```json
{
  "TEMPORAL_VERSION": "1.24.2",
  "TEMPORAL_ADMINTOOLS_VERSION": "1.24.2",
  "TEMPORAL_UI_VERSION": "2.26.2"
}
```

## Documentation & Samples

📖 **Full documentation and samples:** [github.com/fnandop/nando-aspire-temporal](https://github.com/fnandop/nando-aspire-temporal)

## License

MIT
