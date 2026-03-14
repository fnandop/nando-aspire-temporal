using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleWorker;
using Temporalio.Client;
using Xunit;

namespace Nando.Aspire.Temporal.IntegrationTests;





/// <summary>
/// Integration tests for Temporal Aspire hosting.
/// Tests that the Temporal server starts and workflows can be executed.
/// </summary>
public class TemporalIntegrationTests : IAsyncLifetime
{
    private DistributedApplication? _app;
    private ResourceNotificationService? _notificationService;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.DevServer_AppHost>();
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
            // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

         _app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        _notificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task TemporalServer_ShouldStart_AndBecomeHealthy()
    {
        // Arrange & Act
        await _notificationService!.WaitForResourceAsync(
            "temporal",
            KnownResourceStates.Running, 
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        // Assert - if we get here without exception, the server started successfully
        Assert.NotNull(_app);
    }

    [Fact]
    public async Task TemporalClient_ShouldConnect_ToServer()
    {
        // Arrange
        await _notificationService!.WaitForResourceAsync(
            "temporal",
            KnownResourceStates.Running,
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        var temporalEndpoint = await GetTemporalEndpointAsync();

        // Act
        var client = await TemporalClient.ConnectAsync(new TemporalClientConnectOptions(temporalEndpoint));

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.Connection);
    }

    [Fact]
    public async Task Workflow_ShouldStart_AndComplete()
    {
        // Arrange
        await _notificationService!.WaitForResourceAsync(
            "temporal",
            KnownResourceStates.Running,
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        await _notificationService!.WaitForResourceAsync(
            "worker",
            KnownResourceStates.Running,
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        var temporalEndpoint = await GetTemporalEndpointAsync();
        var client = await TemporalClient.ConnectAsync(new TemporalClientConnectOptions(temporalEndpoint));

        // Act
        var result = await client.ExecuteWorkflowAsync(
            (SayHelloWorkflow wf) => wf.RunAsync("IntegrationTest"),
            new WorkflowOptions(
                id: $"test-workflow-{Guid.NewGuid()}",
                taskQueue: "my-task-queue"
            )
        );

        // Assert
        Assert.Equal("Hello, IntegrationTest!", result);
    }

    [Fact]
    public async Task ApiEndpoint_ShouldStartWorkflow_AndReturnResult()
    {
        // Arrange
        await _notificationService!.WaitForResourceAsync(
            "temporal",
            KnownResourceStates.Running,
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        await _notificationService!.WaitForResourceAsync(
            "worker",
            KnownResourceStates.Running,
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        await _notificationService!.WaitForResourceAsync(
            "api",
            KnownResourceStates.Running,
            cancellationToken: TestContext.Current.CancellationToken
        ).WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);

        //var apiEndpoint = await GetApiEndpointAsync();
        //using var httpClient = new HttpClient { BaseAddress = new Uri(apiEndpoint) };
        var httpClient = _app!.CreateHttpClient("api");
        // Act
        var response = await httpClient.GetAsync("/say-hello");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"API returned {response.StatusCode}");
    }

    private async Task<string> GetTemporalEndpointAsync()
    {
        var temporalResource = _app!.Services
            .GetRequiredService<DistributedApplicationModel>()
            .Resources
            .First(r => r.Name == "temporal");

        var endpoint = temporalResource.Annotations
            .OfType<EndpointAnnotation>()
            .First(e => e.Name == "temporal-server");

        var allocatedEndpoint = await _app!.Services
            .GetRequiredService<ResourceNotificationService>()
            .WaitForResourceAsync("temporal", r => r.Snapshot.Urls.Any())
            .WaitAsync(TimeSpan.FromSeconds(30));

        // Get the allocated port from resource URLs
        var url = allocatedEndpoint.Snapshot.Urls
            .FirstOrDefault(u => u.Name == "temporal-server")?.Url;

        if (url is not null)
        {
            var uri = new Uri(url);
            return $"{uri.Host}:{uri.Port}";
        }

        return $"localhost:{endpoint.Port}";
    }

    private async Task<string> GetApiEndpointAsync()
    {
        var allocatedEndpoint = await _notificationService!
            .WaitForResourceAsync("api", r => r.Snapshot.Urls.Any())
            .WaitAsync(TimeSpan.FromSeconds(30));

        var url = allocatedEndpoint.Snapshot.Urls.FirstOrDefault()?.Url;
        return url ?? throw new InvalidOperationException("API endpoint not found");
    }


}
