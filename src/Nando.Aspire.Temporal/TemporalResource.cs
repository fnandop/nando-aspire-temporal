using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

public sealed class TemporalResource(string name) : ContainerResource(name), IResourceWithConnectionString
{

    internal const string TemporalServerGRPCEndpointName = "temporal-server";
    internal const string TemporalServerUIEndpointName = "temporal-server-ui";

    public EndpointReference TemporalServerEndpoint =>
        field ??= new(this, TemporalServerGRPCEndpointName);

    public EndpointReference TemporalServerUiEndpoint =>
        field ??= new(this, TemporalServerUIEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
    ReferenceExpression.Create(
        $"{TemporalServerEndpoint.Property(EndpointProperty.HostAndPort)}"
    );

    /// <summary>
    /// Gets the connection string for the Temporal Server
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        return this.TryGetLastAnnotation<ConnectionStringRedirectAnnotation>(out var connectionStringAnnotation)
            ? connectionStringAnnotation.Resource.GetConnectionStringAsync(cancellationToken)
            : ConnectionStringExpression.GetValueAsync(cancellationToken);
    }
}