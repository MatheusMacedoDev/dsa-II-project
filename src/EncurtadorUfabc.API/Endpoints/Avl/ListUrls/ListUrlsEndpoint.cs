using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.ListUrls;

public class ListUrlsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/avl/urls", ListAsync).WithTags("AVL").WithSummary("Lista todas as URLs armazenadas na arvore AVL (ordenadas por codigo)").AddEndpointFilter(new LogProcessFilter("Avl.ListUrls"));
    }

    public Task<IResult> ListAsync([FromKeyedServices("avl")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<ListUrlsEndpoint> logger, CancellationToken ct)
    {
        var urls = table.Items().Select(item => new ShortUrlResponse(item.Value.Code, item.Value.OriginalUrl, item.Value.CreatedAt, item.Value.AccessCount)).ToArray();
        return Task.FromResult(Results.Ok(urls));
    }
}
