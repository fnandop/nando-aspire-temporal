using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

public sealed class TemporalResource(string name) : ContainerResource(name), IResource
{

    internal const string TemporalServerGRPCEndpointName = "temporal-server";
    internal const string TemporalServerUIEndpointName = "temporal-server-ui";

    public EndpointReference TemporalServerEndpointGRPC =>
        field ??= new(this, TemporalServerGRPCEndpointName);

    public EndpointReference TemporalServerUiEndpoint =>
        field ??= new(this, TemporalServerUIEndpointName);


    public ReferenceExpression TemporalServerGRPCEndpointExpression => ReferenceExpression.Create($"{TemporalServerEndpointGRPC.Property(EndpointProperty.HostAndPort)}");
    public ReferenceExpression TemporalServerUiEndpointExpression => ReferenceExpression.Create($"{TemporalServerUiEndpoint.Property(EndpointProperty.HostAndPort)}");

}