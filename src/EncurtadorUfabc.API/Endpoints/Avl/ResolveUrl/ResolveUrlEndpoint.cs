using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.ResolveUrl;

public class ResolveUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/avl/urls/{code}", ResolveAsync).WithTags("AVL").WithSummary("Resolve um codigo e redireciona para a URL original (use ?raw=true para retornar JSON)").AddEndpointFilter(new LogProcessFilter("Avl.ResolveUrl"));
    }

    public Task<IResult> ResolveAsync([FromRoute] string code, [FromQuery] bool raw, [FromKeyedServices("avl")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<ResolveUrlEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
