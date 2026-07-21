using Microsoft.AspNetCore.Routing;

namespace EncurtadorUfabc.Core.Crosscutting;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}
