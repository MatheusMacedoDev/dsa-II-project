using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EncurtadorUfabc.Core.Crosscutting;

public class LogProcessFilter(string processName) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Process");
        using (logger.BeginScope(new Dictionary<string, object> { ["Process"] = processName }))
            return await next(context);
    }
}
