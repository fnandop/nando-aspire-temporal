using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

public static class ITemporalResourceExtensions
{
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
}