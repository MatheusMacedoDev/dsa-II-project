using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EncurtadorUfabc.Core.Crosscutting;

public static class EndpointExtensions
{
    public static void UseEndpoints(this WebApplication app)
    {
        var endpointTypes = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())
            .DefinedTypes
            .Where(type => type is { IsInterface: false, IsAbstract: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .ToArray();

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)ActivatorUtilities.CreateInstance(app.Services, type);
            endpoint.Map(app);
        }
    }
}
