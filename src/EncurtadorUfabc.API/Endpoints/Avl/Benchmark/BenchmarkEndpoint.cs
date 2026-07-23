using EncurtadorUfabc.AVL;
using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Models;
using EncurtadorUfabc.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EncurtadorUfabc.API.Endpoints.Avl.Benchmark;

public class BenchmarkEndpoint : IEndpoint
{
    private readonly IBenchmarkService _benchmark;

    public BenchmarkEndpoint(IBenchmarkService benchmark)
    {
        _benchmark = benchmark;
    }

    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/avl/benchmark", BenchmarkAsync).WithTags("AVL").WithSummary("Executa um benchmark das operacoes da arvore AVL").AddEndpointFilter(new LogProcessFilter("Avl.Benchmark"));
    }

    public Task<IResult> BenchmarkAsync([FromQuery] int operations, ILogger<BenchmarkEndpoint> logger, CancellationToken ct)
    {
        if (operations <= 0)
            return Task.FromResult(Results.BadRequest("O numero de operacoes deve ser maior que zero."));

        var table = new AvlSymbolTable<string, string>();
        var response = _benchmark.Run(table, operations, "AVL",
            () => new StructureSnapshot(table.Count, table.Height, null, null, null));
        return Task.FromResult(Results.Ok(response));
    }
}
