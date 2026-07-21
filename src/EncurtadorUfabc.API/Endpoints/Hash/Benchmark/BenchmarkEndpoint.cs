using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Hash.Benchmark;

public class BenchmarkEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/hash/benchmark", BenchmarkAsync).WithTags("Hash").WithSummary("Executa um benchmark das operacoes da tabela hash").AddEndpointFilter(new LogProcessFilter("Hash.Benchmark"));
    }

    public Task<IResult> BenchmarkAsync([FromQuery] int operations, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, ILogger<BenchmarkEndpoint> logger, CancellationToken ct) => throw new NotImplementedException();
}
