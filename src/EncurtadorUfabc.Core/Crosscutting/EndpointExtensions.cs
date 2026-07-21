using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace EncurtadorUfabc.Core.Crosscutting;

public static class EndpointExtensions
{
    public static void UseEndpoints(this WebApplication app)
    {
        var endpoints = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())
            .DefinedTypes
            .Where(type => type is { IsInterface: false, IsAbstract: false } && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => Activator.CreateInstance(type) as IEndpoint ?? throw new InvalidOperationException($"Could not create instance of IEndpoint {type.Name}"))
            .ToArray();

        foreach (var endpoint in endpoints)
            endpoint.Map(app);
    }
}
