using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

public sealed class ContainerTemporalResource(string name) : ContainerResource(name), ITemporalResource
{
    internal const string TemporalServerGRPCEndpointName = "temporal-server";
    internal const string TemporalServerUIEndpointName = "temporal-server-ui";

    public static string DefaultDynamicConfig => """
        limit.maxIDLength:
          - value: 255
            constraints: {}
        system.forceSearchAttributesCacheRefreshOnRead:
          - value: true
            constraints: {}
        """;

    public EndpointReference TemporalServerEndpointGRPC =>
        field ??= new(this, TemporalServerGRPCEndpointName);

    public ReferenceExpression TemporalServerGRPCEndpointExpression => ReferenceExpression.Create($"{TemporalServerEndpointGRPC.Property(EndpointProperty.HostAndPort)}");
    public string? Domain { get; set; } = "default";

}