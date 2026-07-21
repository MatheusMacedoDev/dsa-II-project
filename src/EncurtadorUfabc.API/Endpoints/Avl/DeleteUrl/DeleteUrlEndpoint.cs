using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.DeleteUrl;

public class DeleteUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/avl/urls/{code}", DeleteAsync).WithTags("AVL").WithSummary("Remove um codigo da arvore AVL").AddEndpointFilter(new LogProcessFilter("Avl.DeleteUrl"));
    }

    public Task<IResult> DeleteAsync([FromRoute] string code, [FromKeyedServices("avl")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<DeleteUrlEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
