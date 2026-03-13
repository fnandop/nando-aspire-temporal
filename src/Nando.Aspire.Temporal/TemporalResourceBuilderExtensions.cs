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
    public static IResourceBuilder<TemporalResource> AddTemporal(this IDistributedApplicationBuilder builder, string name, int? grpcPort = null)
    {
        string temporalVersion = builder.Configuration["TEMPORAL_VERSION"] ?? "latest";
        var resource = new TemporalResource(name);

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
                name: TemporalResource.TemporalServerGRPCEndpointName,
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
    public static IResourceBuilder<TemporalResource> WithPostgres(this IResourceBuilder<TemporalResource> builder,
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
    public static IResourceBuilder<TemporalResource> WithtTemporalAdminTools(this IResourceBuilder<TemporalResource> temporal)
    {
        var builder = temporal.ApplicationBuilder;
        string temporalAdminToolsVersion = builder.Configuration["TEMPORAL_ADMINTOOLS_VERSION"] ?? "latest";

        _ = builder
            .AddContainer("temporal-admin-tools", "temporalio/admin-tools", temporalAdminToolsVersion)
            .WithContainerName("temporal-admin-tools")
            .WithReference(temporal)
            .WithEnvironment("TEMPORAL_ADDRESS", temporal.Resource.ConnectionStringExpression)
            .WithEnvironment("TEMPORAL_CLI_ADDRESS", temporal.Resource.ConnectionStringExpression);

        return temporal;
    }

    /// <summary>
    /// Adds the Temporal UI to the application.
    /// </summary>
    /// <param name="temporal">The Temporal resource builder.</param>
    /// <param name="uiPort">The port for the Temporal UI (optional).</param>
    /// <returns>An <see cref="IResourceBuilder{TemporalResource}"/> for further configuration.</returns>
    public static IResourceBuilder<TemporalResource> WithtTemporalUi(this IResourceBuilder<TemporalResource> temporal, int? uiPort = null)
    {
        var builder = temporal.ApplicationBuilder;
        string temporalUiVersion = builder.Configuration["TEMPORAL_UI_VERSION"] ?? "latest";

        _ = builder
            .AddContainer("temporal-ui", "temporalio/ui", temporalUiVersion)
            .WithContainerName("temporal-ui")
            .WithReference(temporal)
            .WithEnvironment("TEMPORAL_ADDRESS", temporal.Resource.ConnectionStringExpression)
            .WithEnvironment("TEMPORAL_CORS_ORIGINS", "http://localhost:3000")
            .WithEndpoint(
                name: TemporalResource.TemporalServerUIEndpointName,
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
    public static IResourceBuilder<TemporalResource> AddTemporalDevServer(this IDistributedApplicationBuilder builder, string name, int? grpcPort = null, int? uiPort = null)
    {
        string temporalVersion = builder.Configuration["TEMPORAL_VERSION"] ?? "latest";
        var resource = new TemporalResource(name);

        return builder.AddResource(resource)
            .WithImage("temporalio/temporal")
            .WithImageRegistry("docker.io")
            .WithImageTag(temporalVersion)
            .WithArgs("server", "start-dev", "--ip", "0.0.0.0")
            .WithEndpoint(
                name: TemporalResource.TemporalServerGRPCEndpointName,
                port: grpcPort ?? 7233,
                targetPort: 7233,
                scheme: "grpc"
            )
            .WithEndpoint(
                name: TemporalResource.TemporalServerUIEndpointName,
                port: uiPort ?? 8233,
                targetPort: 8233,
                scheme: "http"
            );
    }
}