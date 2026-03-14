using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

public sealed class ContainerTemporalResource(string name) : ContainerResource(name), ITemporalResource
{

    internal const string TemporalServerGRPCEndpointName = "temporal-server";
    internal const string TemporalServerUIEndpointName = "temporal-server-ui";

    public EndpointReference TemporalServerEndpointGRPC =>
        field ??= new(this, TemporalServerGRPCEndpointName);

    public ReferenceExpression TemporalServerGRPCEndpointExpression => ReferenceExpression.Create($"{TemporalServerEndpointGRPC.Property(EndpointProperty.HostAndPort)}");
    public string? Domain { get; set; } = "default";

}