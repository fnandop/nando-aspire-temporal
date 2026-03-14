using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

/// <summary>
/// Provides extension methods for configuring Temporal resources in a distributed application.
/// </summary>
public static class TemporalResourceBuilderExtensions
{
    /// <summary>
    /// Adds a Temporal resource to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the Temporal resource.</param>
    /// <param name="grpcPort">The gRPC port for the Temporal server (optional).</param>
    /// <returns>An <see cref="IResourceBuilder{TemporalResource}"/> for further configuration.</returns>
    public static IResourceBuilder<ContainerTemporalResource> AddTemporal(this IDistributedApplicationBuilder builder, string name, int? grpcPort = null, string? domain = null)
    {
        string temporalVersion = builder.Configuration["TEMPORAL_VERSION"] ?? "latest";
        var resource = new ContainerTemporalResource(name);
        resource.Domain ??= domain;

        var temporal = builder.AddResource(resource)
            .WithImage("temporalio/auto-setup", temporalVersion)
            .WithContainerName("temporal")
            .WithEnvironment("DYNAMIC_CONFIG_FILE_PATH", "config/dynamicconfig/development-sql.yaml")
            .WithBindMount(
                source: "./dynamicconfig",
                target: "/etc/temporal/config/dynamicconfig",
                isReadOnly: false
            )
            .WithEndpoint(
                name: ContainerTemporalResource.TemporalServerGRPCEndpointName,
                port: grpcPort ?? 7233,
                targetPort: 7233,
                scheme: "grpc"
            );

        return temporal;
    }

    /// <summary>
    /// Configures the Temporal resource to use a PostgreSQL database.
    /// </summary>
    /// <param name="builder">The Temporal resource builder.</param>
    /// <param name="postgres">The PostgreSQL resource builder.</param>
    /// <returns>An <see cref="IResourceBuilder{TemporalResource}"/> for further configuration.</returns>
    public static IResourceBuilder<ContainerTemporalResource> WithPostgres(this IResourceBuilder<ContainerTemporalResource> builder,
                                                                   IResourceBuilder<PostgresServerResource> postgres)
    {
        var postgresDatabase = postgres.Resource;
        _ = builder
            .WithReference(postgres)
            .WaitFor(postgres)
            .WithEnvironment("DB", "postgres12")
            .WithEnvironment("DB_PORT", postgresDatabase.PrimaryEndpoint.TargetPort.ToString())
            .WithEnvironment("POSTGRES_USER", postgresDatabase.UserNameReference)
            .WithEnvironment("POSTGRES_PWD", postgresDatabase.PasswordParameter)
            .WithEnvironment("POSTGRES_SEEDS", postgresDatabase.Name);

        return builder;
    }

    /// <summary>
    /// Adds Temporal Admin Tools to the application.
    /// </summary>
    /// <param name="temporal">The Temporal resource builder.</param>
    /// <returns>An <see cref="IResourceBuilder{TemporalResource}"/> for further configuration.</returns>
    public static IResourceBuilder<ContainerTemporalResource> WithtTemporalAdminTools(this IResourceBuilder<ContainerTemporalResource> temporal)
    {
        var builder = temporal.ApplicationBuilder;
        string temporalAdminToolsVersion = builder.Configuration["TEMPORAL_ADMINTOOLS_VERSION"] ?? "latest";

        _ = builder
            .AddContainer("temporal-admin-tools", "temporalio/admin-tools", temporalAdminToolsVersion)
            .WithContainerName("temporal-admin-tools")
            .WithReference(temporal)
            .WithEnvironment("TEMPORAL_ADDRESS", temporal.Resource.TemporalServerGRPCEndpointExpression)
            .WithEnvironment("TEMPORAL_CLI_ADDRESS", temporal.Resource.TemporalServerGRPCEndpointExpression);

        return temporal;
    }

    /// <summary>
    /// Adds the Temporal UI to the application.
    /// </summary>
    /// <param name="temporal">The Temporal resource builder.</param>
    /// <param name="uiPort">The port for the Temporal UI (optional).</param>
    /// <returns>An <see cref="IResourceBuilder{TemporalResource}"/> for further configuration.</returns>
    public static IResourceBuilder<ContainerTemporalResource> WithtTemporalUi(this IResourceBuilder<ContainerTemporalResource> temporal, int? uiPort = null)
    {
        var builder = temporal.ApplicationBuilder;
        string temporalUiVersion = builder.Configuration["TEMPORAL_UI_VERSION"] ?? "latest";

        _ = builder
            .AddContainer("temporal-ui", "temporalio/ui", temporalUiVersion)
            .WithContainerName("temporal-ui")
            .WithReference(temporal)
            .WithEnvironment("TEMPORAL_ADDRESS", temporal.Resource.TemporalServerGRPCEndpointExpression)
            .WithEnvironment("TEMPORAL_CORS_ORIGINS", "http://localhost:3000")
            .WithEndpoint(
                name: ContainerTemporalResource.TemporalServerUIEndpointName,
                port: uiPort ?? 8233,
                targetPort: 8080,
                scheme: "http"
            );

        return temporal;
    }

    /// <summary>
    /// Adds a Temporal development server to the distributed application builder.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the Temporal resource.</param>
    /// <param name="grpcPort">The gRPC port for the Temporal server (optional).</param>
    /// <param name="uiPort">The port for the Temporal UI (optional).</param>
    /// <returns>An <see cref="IResourceBuilder{TemporalResource}"/> for further configuration.</returns>
    public static IResourceBuilder<ContainerTemporalResource> AddTemporalDevServer(this IDistributedApplicationBuilder builder, string name, int? grpcPort = null, int? uiPort = null, string? domain = null)
    {
        string temporalVersion = builder.Configuration["TEMPORAL_VERSION"] ?? "latest";
        var resource = new ContainerTemporalResource(name);
        resource.Domain ??= domain;

        return builder.AddResource(resource)
            .WithImage("temporalio/temporal")
            .WithImageRegistry("docker.io")
            .WithImageTag(temporalVersion)
            .WithArgs("server", "start-dev", "--ip", "0.0.0.0")
            .WithEndpoint(
                name: ContainerTemporalResource.TemporalServerGRPCEndpointName,
                port: grpcPort ?? 7233,
                targetPort: 7233,
                scheme: "grpc"
            )
            .WithEndpoint(
                name: ContainerTemporalResource.TemporalServerUIEndpointName,
                port: uiPort ?? 8233,
                targetPort: 8233,
                scheme: "http"
            );
    }


    private static IResourceBuilder<TDestination> WithReference<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<ContainerTemporalResource> temporal,
        string? name = null)
        where TDestination : IResourceWithEnvironment
    {
        var resource = temporal.Resource;
        name ??= resource.Name;

        builder.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables[$"TEMPORAL_ADDRESS"] =
                temporal.Resource.TemporalServerGRPCEndpointExpression;
            context.EnvironmentVariables[$"TEMPORAL_NAMESPACE"] = temporal.Resource.Domain;

            return Task.CompletedTask;
        }));

        return builder;
    }

    public static IResourceBuilder<TDestination> WithReference<TDestination>(
this IResourceBuilder<TDestination> builder,
IResourceBuilder<ITemporalResource> temporal,
string? name = null)
where TDestination : IResourceWithEnvironment
    {
        if (temporal is IResourceBuilder<ContainerTemporalResource> containerTemporal)
            builder.WithReference(containerTemporal, name);

        if (temporal is IResourceBuilder<CloudTemporalResource> cloudTemporal)
            builder.WithReference(cloudTemporal, name);
        return builder;
    }


    public static IResourceBuilder<CloudTemporalResource> AddTemporalCloud(this IDistributedApplicationBuilder builder, string name, string temporalServerGRPCEndpoint, string domain)
    {

        var resource = new CloudTemporalResource(name) { TemporalServerGRPCEndpoint = temporalServerGRPCEndpoint, Domain = domain };
        var resourceBuilder = builder.AddResource(resource)
             .WithInitialState(new CustomResourceSnapshot
             {
                 ResourceType = "TemporalCloudService",
                 State = KnownResourceStates.Waiting,
                 Properties = []
             })
             .ExcludeFromManifest();

        // Subscribe to the InitializeResourceEvent to finish setting up the resource
        builder.Eventing.Subscribe<InitializeResourceEvent>(resource, static async (e, ct) =>
        {
            var resource = e.Resource as CloudTemporalResource;
            if (resource is not null)
            {

                await e.Eventing.PublishAsync(new BeforeResourceStartedEvent(e.Resource, e.Services), ct).ConfigureAwait(false);

                await e.Notifications.PublishUpdateAsync(resource, snapshot => snapshot with
                {
                    Urls = [new UrlSnapshot("Temporal Cloud", resource.TemporalServerGRPCEndpoint, false)],

                    //// Add the URL if it came from a parameter as non-static URLs must be published by the owning custom resource
                    //Urls = AddUrlIfNotPresent(snapshot.Urls, uri),
                    // Required in order for health checks to work
                    State = KnownResourceStates.Running
                }).ConfigureAwait(false);


            }
        });

        return resourceBuilder;

    }






    private static IResourceBuilder<TDestination> WithReference<TDestination>(
    this IResourceBuilder<TDestination> builder,
    IResourceBuilder<CloudTemporalResource> temporal,
    string? name = null)
    where TDestination : IResourceWithEnvironment
    {
        var resource = temporal.Resource;
        name ??= resource.Name;

        builder.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables[$"TEMPORAL_ADDRESS"] =
                temporal.Resource.TemporalServerGRPCEndpoint;
            context.EnvironmentVariables[$"TEMPORAL_NAMESPACE"] = temporal.Resource.Domain;

            return Task.CompletedTask;
        }));

        return builder;
    }


}