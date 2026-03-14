using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;


public interface ITemporalResource : IResource
{
}

public sealed class ContainerTemporalResource(string name) : ContainerResource(name), ITemporalResource
{

    internal const string TemporalServerGRPCEndpointName = "temporal-server";
    internal const string TemporalServerUIEndpointName = "temporal-server-ui";

    public EndpointReference TemporalServerEndpointGRPC =>
        field ??= new(this, TemporalServerGRPCEndpointName);

    public ReferenceExpression TemporalServerGRPCEndpointExpression => ReferenceExpression.Create($"{TemporalServerEndpointGRPC.Property(EndpointProperty.HostAndPort)}");
    public  string? Domain { get; set; } = "default";

}


public sealed class CloudTemporalResource(string name) : Resource(name), ITemporalResource
{
    public required string TemporalServerGRPCEndpoint { get; init; }
    public required string Domain { get; init; }

}