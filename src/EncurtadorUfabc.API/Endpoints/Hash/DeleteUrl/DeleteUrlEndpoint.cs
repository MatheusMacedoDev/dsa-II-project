using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Hash.DeleteUrl;

public class DeleteUrlEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/hash/urls/{code}", DeleteAsync).WithTags("Hash").WithSummary("Remove um codigo da tabela hash").AddEndpointFilter(new LogProcessFilter("Hash.DeleteUrl"));
    }

    public Task<IResult> DeleteAsync([FromRoute] string code, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, AppDbContext db, ILogger<DeleteUrlEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
