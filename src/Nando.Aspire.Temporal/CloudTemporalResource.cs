using Aspire.Hosting.ApplicationModel;

namespace Nando.Aspire.Temporal;

public sealed class CloudTemporalResource(string name) : Resource(name), ITemporalResource
{
    public required ParameterResource ServerEndpointParameter { get; init; }
    public required ParameterResource DomainParameter { get; init; }

}