# Temporal Integration Tests

Integration tests that verify the Temporal Aspire hosting works correctly with real containers.

## Prerequisites

- Docker Desktop running and healthy
- .NET 10 SDK

## What's Tested

| Test | Description |
|------|-------------|
| `TemporalServer_ShouldStart_AndBecomeHealthy` | Verifies Temporal container starts and becomes healthy |
| `TemporalClient_ShouldConnect_ToServer` | Verifies a Temporal client can connect to the server |
| `Workflow_ShouldStart_AndComplete` | Verifies a workflow can be started and completed |
| `ApiEndpoint_ShouldStartWorkflow_AndReturnResult` | Verifies the API can trigger workflows via HTTP |

## Running Tests

### Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Run all tests or select specific tests

### Command Line

```bash
# Run all integration tests
dotnet test tests/Nando.Aspire.Temporal.IntegrationTests

# Run a specific test
dotnet test tests/Nando.Aspire.Temporal.IntegrationTests --filter "FullyQualifiedName~TemporalServer_ShouldStart"

# Run with verbose output
dotnet test tests/Nando.Aspire.Temporal.IntegrationTests --verbosity normal
```

## Test Architecture

The tests use **Aspire.Hosting.Testing** to:
1. Start the `DevServer.AppHost` project
2. Wait for resources to become healthy
3. Connect to Temporal and execute workflows
4. Verify results

```
┌─────────────────────────────────────────────────────────┐
│                    Test Process                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │            DevServer.AppHost                      │  │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐  │  │
│  │  │  Temporal   │ │   Worker    │ │    API      │  │  │
│  │  │  Container  │ │   Project   │ │   Project   │  │  │
│  │  └─────────────┘ └─────────────┘ └─────────────┘  │  │
│  └───────────────────────────────────────────────────┘  │
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │              Integration Tests                    │  │
│  │  • Connect to Temporal                            │  │
│  │  • Execute workflows                              │  │
│  │  • Call API endpoints                             │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Troubleshooting

### Docker not running
```
Container runtime 'docker' was found but appears to be unhealthy
```
**Solution:** Start Docker Desktop and ensure it's fully running.

### Tests timeout
Increase the timeout in the test or ensure your machine has enough resources:
```csharp
.WaitAsync(TimeSpan.FromSeconds(120));
```

### Port conflicts
If default ports are in use, modify the AppHost to use different ports.
