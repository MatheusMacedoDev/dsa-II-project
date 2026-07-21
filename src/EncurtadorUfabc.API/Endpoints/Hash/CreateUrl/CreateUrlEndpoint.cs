using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Hash.CreateUrl;

public class CreateUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/hash/urls", CreateAsync).WithTags("Hash").WithSummary("Cria uma URL curta usando a tabela hash").AddEndpointFilter(new LogProcessFilter("Hash.CreateUrl"));
    }

    public Task<IResult> CreateAsync([FromBody] CreateUrlRequest request, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<CreateUrlEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
