using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace Nando.Aspire.Temporal;

public static class CloudTemporalResourceBuilderExtensions
{

    public static IResourceBuilder<CloudTemporalResource> AddTemporalCloud(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>  serverEndpointParameter, IResourceBuilder<ParameterResource> domainParameter)
    {

        var resource = new CloudTemporalResource(name) { ServerEndpointParameter = serverEndpointParameter.Resource, DomainParameter = domainParameter.Resource };
        var resourceBuilder = builder.AddResource(resource)
             .WithInitialState(new CustomResourceSnapshot
             {
                 ResourceType = "TemporalCloudService",
                 State = KnownResourceStates.Waiting,
                 Properties = []
             })
             .ExcludeFromManifest();

        resourceBuilder.WithUrl(ReferenceExpression.Create($"{resource.ServerEndpointParameter}"));


        // Subscribe to the InitializeResourceEvent to finish setting up the resource
        builder.Eventing.Subscribe<InitializeResourceEvent>(resource, static async (e, ct) =>
        {
            var resource = e.Resource as CloudTemporalResource;
            if (resource is not null)
            {

                string? serverEndpoint;
                try
                {
                    serverEndpoint = await resource.ServerEndpointParameter.GetValueAsync(ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    e.Logger.LogError(ex, "Failed to get value for URL parameter '{ParameterName}'", resource.ServerEndpointParameter?.Name);

                    await e.Notifications.PublishUpdateAsync(resource, snapshot => snapshot with
                    {
                        State = KnownResourceStates.FailedToStart
                    }).ConfigureAwait(false);

                    return;
                }

                string? domain;
                try
                {
                    domain = await resource.DomainParameter.GetValueAsync(ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    e.Logger.LogError(ex, "Failed to get value for URL parameter '{ParameterName}'", resource.DomainParameter?.Name);

                    await e.Notifications.PublishUpdateAsync(resource, snapshot => snapshot with
                    {
                        State = KnownResourceStates.FailedToStart
                    }).ConfigureAwait(false);

                    return;
                }


                await e.Eventing.PublishAsync(new BeforeResourceStartedEvent(e.Resource, e.Services), ct).ConfigureAwait(false);

                await e.Notifications.PublishUpdateAsync(resource, snapshot => snapshot with
                {
                    Urls = [new UrlSnapshot("Temporal Cloud", serverEndpoint!, false)],
                    Properties = [new ResourcePropertySnapshot("Domain", domain!)],
                    State = KnownResourceStates.Running
                }).ConfigureAwait(false);


            }
        });

        return resourceBuilder;

    }

    internal static IResourceBuilder<TDestination> WithReference<TDestination>(
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
                temporal.Resource.ServerEndpointParameter;
            context.EnvironmentVariables[$"TEMPORAL_NAMESPACE"] = temporal.Resource.DomainParameter;

            return Task.CompletedTask;
        }));

        return builder;
    }
}