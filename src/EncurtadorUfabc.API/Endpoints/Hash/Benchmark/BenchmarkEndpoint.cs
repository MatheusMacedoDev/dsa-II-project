using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Models;
using EncurtadorUfabc.Core.Services;
using EncurtadorUfabc.Hash;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Hash.Benchmark;

public class BenchmarkEndpoint : IEndpoint
{
    private readonly IBenchmarkService _benchmark;

    public BenchmarkEndpoint(IBenchmarkService benchmark)
    {
        _benchmark = benchmark;
    }

    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/hash/benchmark", BenchmarkAsync).WithTags("Hash").WithSummary("Executa um benchmark das operacoes da tabela hash").AddEndpointFilter(new LogProcessFilter("Hash.Benchmark"));
    }

    public Task<IResult> BenchmarkAsync([FromQuery] int operations, ILogger<BenchmarkEndpoint> logger, CancellationToken ct)
    {
        if (operations <= 0)
            return Task.FromResult(Results.BadRequest("O numero de operacoes deve ser maior que zero."));

        var response = _benchmark.Run(
            () => new HashSymbolTable<string, string>(),
            operations,
            "Hash",
            table => new StructureSnapshot(table.Count, null, ((HashSymbolTable<string, string>)table).BucketCount, ((HashSymbolTable<string, string>)table).LoadFactor, ((HashSymbolTable<string, string>)table).MaxChainLength));
        return Task.FromResult(Results.Ok(response));
    }
}
