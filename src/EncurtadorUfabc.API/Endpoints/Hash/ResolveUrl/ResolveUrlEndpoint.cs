using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Hash.ResolveUrl;

public class ResolveUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/hash/urls/{code}", ResolveAsync).WithTags("Hash").WithSummary("Resolve um codigo e redireciona para a URL original (use ?raw=true para retornar JSON)").AddEndpointFilter(new LogProcessFilter("Hash.ResolveUrl"));
    }

    public Task<IResult> ResolveAsync([FromRoute] string code, [FromQuery] bool raw, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<ResolveUrlEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
