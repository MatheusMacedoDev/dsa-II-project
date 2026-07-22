using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.DeleteUrl;

public class DeleteUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/avl/urls/{code}", DeleteAsync).WithTags("AVL").WithSummary("Remove um codigo da arvore AVL").AddEndpointFilter(new LogProcessFilter("Avl.DeleteUrl"));
    }

    public async Task<IResult> DeleteAsync([FromRoute] string code, [FromKeyedServices("avl")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<DeleteUrlEndpoint> logger, CancellationToken ct)
    {
        if (!table.Delete(code))
            return Results.NotFound();

        var entity = await db.ShortUrls.FindAsync(new object?[] { code }, ct);
        if (entity is not null)
        {
            db.ShortUrls.Remove(entity);
            await db.SaveChangesAsync(ct);
        }

        return Results.NoContent();
    }
}
