using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Hash.ResolveUrl;

public class ResolveUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/hash/urls/{code}", ResolveAsync).WithTags("Hash").WithSummary("Resolve um codigo e redireciona para a URL original (use ?raw=true para retornar JSON)").AddEndpointFilter(new LogProcessFilter("Hash.ResolveUrl"));
    }

    public async Task<IResult> ResolveAsync([FromRoute] string code, [FromQuery] bool? raw, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<ResolveUrlEndpoint> logger, CancellationToken ct)
    {
        if (!table.TryGet(code, out var shortUrl))
            return Results.NotFound();

        shortUrl.AccessCount++;

        var entity = await db.ShortUrls.FindAsync([code], ct);
        if (entity is not null)
        {
            entity.AccessCount = shortUrl.AccessCount;
            await db.SaveChangesAsync(ct);
        }

        if (raw == true)
            return Results.Ok(new ShortUrlResponse(shortUrl.Code, shortUrl.OriginalUrl, shortUrl.CreatedAt, shortUrl.AccessCount));

        return Results.Redirect(shortUrl.OriginalUrl);
    }
}
