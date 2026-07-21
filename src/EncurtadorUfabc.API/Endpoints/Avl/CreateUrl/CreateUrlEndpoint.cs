using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.CreateUrl;

public class CreateUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/avl/urls", CreateAsync).WithTags("AVL").WithSummary("Cria uma URL curta usando a arvore AVL").AddEndpointFilter(new LogProcessFilter("Avl.CreateUrl"));
    }

    public Task<IResult> CreateAsync([FromBody] CreateUrlRequest request, [FromKeyedServices("avl")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<CreateUrlEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
