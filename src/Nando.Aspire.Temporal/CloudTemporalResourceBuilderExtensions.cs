using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace Nando.Aspire.Temporal;

public static class CloudTemporalResourceBuilderExtensions
{
    public static IResourceBuilder<CloudTemporalResource> AddTemporalCloud(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<ParameterResource> serverEndpointParameter,
        IResourceBuilder<ParameterResource> domainParameter,
        IResourceBuilder<ParameterResource> apiKeyParameter)
    {
        var resource = new CloudTemporalResource(name)
        {
            ServerEndpointParameter = serverEndpointParameter.Resource,
            DomainParameter = domainParameter.Resource,
            ApiKeyParameter = apiKeyParameter.Resource
        };

        var resourceBuilder = builder.AddResource(resource)
            .WithInitialState(new CustomResourceSnapshot
            {
                ResourceType = "TemporalCloudService",
                State = KnownResourceStates.Waiting,
                Properties = []
            })
            .ExcludeFromManifest();

        resourceBuilder.WithUrl(ReferenceExpression.Create($"{resource.ServerEndpointParameter}"));

        builder.Eventing.Subscribe<InitializeResourceEvent>(resource, static async (e, ct) =>
        {
            if (e.Resource is not CloudTemporalResource resource)
            {
                return;
            }

            var serverEndpoint = await TryGetParameterValueAsync(resource.ServerEndpointParameter, resource, e, ct);
            if (serverEndpoint is null) return;

            var domain = await TryGetParameterValueAsync(resource.DomainParameter, resource, e, ct);
            if (domain is null) return;

            var apiKey = await TryGetParameterValueAsync(resource.ApiKeyParameter, resource, e, ct);
            if (apiKey is null) return;

            await e.Eventing.PublishAsync(new BeforeResourceStartedEvent(e.Resource, e.Services), ct).ConfigureAwait(false);

            await e.Notifications.PublishUpdateAsync(resource, snapshot => snapshot with
            {
                Urls = [new UrlSnapshot("Temporal Cloud", serverEndpoint, false)],
                Properties = [new ResourcePropertySnapshot("Domain", domain)],
                State = KnownResourceStates.Running
            }).ConfigureAwait(false);
        });

        return resourceBuilder;
    }

    private static async Task<string?> TryGetParameterValueAsync(
        ParameterResource parameter,
        CloudTemporalResource resource,
        InitializeResourceEvent e,
        CancellationToken ct)
    {
        try
        {
            return await parameter.GetValueAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            e.Logger.LogError(ex, "Failed to get value for parameter '{ParameterName}'", parameter.Name);

            await e.Notifications.PublishUpdateAsync(resource, snapshot => snapshot with
            {
                State = KnownResourceStates.FailedToStart
            }).ConfigureAwait(false);

            return null;
        }
    }

    /// <summary>
    /// Adds a reference to the Temporal Cloud resource, injecting connection environment variables.
    /// </summary>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(
        this IResourceBuilder<TDestination> builder,
        IResourceBuilder<CloudTemporalResource> temporal,
        string? name = null)
        where TDestination : IResourceWithEnvironment
    {
        var resource = temporal.Resource;
        name ??= resource.Name;

        builder.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables[$"TEMPORAL_ADDRESS"] = temporal.Resource.ServerEndpointParameter;
            context.EnvironmentVariables[$"TEMPORAL_NAMESPACE"] = temporal.Resource.DomainParameter;
            context.EnvironmentVariables[$"TEMPORAL_API_KEY"] = temporal.Resource.ApiKeyParameter;

            return Task.CompletedTask;
        }));

        return builder;
    }
}