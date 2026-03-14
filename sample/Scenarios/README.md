# Sample Scenarios

Each scenario is a fully runnable Aspire AppHost project. Open the solution in Visual Studio and set any scenario as the startup project.

## Scenarios

| Project | Description |
|---------|-------------|
| **DevServer.AppHost** | Simplest setup - lightweight in-memory Temporal server |
| **DevServerCustomPorts.AppHost** | Dev server with custom gRPC/UI ports and namespace |
| **FullContainerStack.AppHost** | Production-like setup with Postgres, Admin Tools, and UI |
| **TemporalCloud.AppHost** | Connect to Temporal Cloud with API key authentication |
| **EnvironmentSwitch.AppHost** | Switch between dev server and cloud based on environment |
| **CustomImageVersions.AppHost** | Pin specific versions of Temporal container images |
| **MinimalWorkerOnly.AppHost** | Just Temporal + a single worker for quick testing |
| **MultiScenario.AppHost** | Single project with config-driven scenario selection |

## Running a Scenario

### Visual Studio
1. Right-click on a scenario project (e.g., `DevServer.AppHost`)
2. Select **Set as Startup Project**
3. Press F5 or click **Start**

### Command Line
```bash
cd sample/Scenarios/DevServer
dotnet run
```

## Environment Variables

For container-based scenarios, you can control image versions:

```bash
# PowerShell
$env:TEMPORAL_VERSION = "1.24.2"
$env:TEMPORAL_ADMINTOOLS_VERSION = "1.24.2"
$env:TEMPORAL_UI_VERSION = "2.26.2"
```

Or set them in `appsettings.json`:

```json
{
  "TEMPORAL_VERSION": "1.24.2",
  "TEMPORAL_ADMINTOOLS_VERSION": "1.24.2",
  "TEMPORAL_UI_VERSION": "2.26.2"
}
```

## Temporal Cloud Setup

For cloud scenarios, configure parameters in `appsettings.json`:

```json
{
  "Parameters": {
    "temporal-server-endpoint": "your-namespace.tmprl.cloud:7233",
    "temporal-domain": "your-namespace"
  }
}
```

Store the API key securely using user secrets:

```bash
dotnet user-secrets set "Parameters:temporal-api-key" "your-api-key"
```
