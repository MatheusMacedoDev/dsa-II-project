using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.Benchmark;

public class BenchmarkEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/avl/benchmark", BenchmarkAsync).WithTags("AVL").WithSummary("Executa um benchmark das operacoes da arvore AVL").AddEndpointFilter(new LogProcessFilter("Avl.Benchmark"));
    }

    public Task<IResult> BenchmarkAsync([FromQuery] int operations, [FromKeyedServices("avl")] ISymbolTable<string, ShortUrl> table, ILogger<BenchmarkEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
