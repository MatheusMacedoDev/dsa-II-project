using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Models;
using EncurtadorUfabc.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EncurtadorUfabc.API.Endpoints.Hash.Benchmark;

public class BenchmarkEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/hash/benchmark", BenchmarkAsync).WithTags("Hash").WithSummary("Executa um benchmark das operacoes da tabela hash").AddEndpointFilter(new LogProcessFilter("Hash.Benchmark"));
    }

    public Task<IResult> BenchmarkAsync([FromQuery] int operations, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, ILogger<BenchmarkEndpoint> logger, CancellationToken ct)
    {
        if (operations <= 0)
            return Task.FromResult(Results.BadRequest("O numero de operacoes deve ser maior que zero."));

        var benchmarkTable = new HashSymbolTable<string, string>();
        var keys = new string[operations];
        for (int index = 0; index < operations; index++)
            keys[index] = $"benchmark-key-{index}";

        var putWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            benchmarkTable.Put(keys[index], keys[index]);
        putWatch.Stop();

        var getWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            benchmarkTable.TryGet(keys[index], out _);
        getWatch.Stop();

        var deleteWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            benchmarkTable.Delete(keys[index]);
        deleteWatch.Stop();

        var totalMs = putWatch.Elapsed.TotalMilliseconds + getWatch.Elapsed.TotalMilliseconds + deleteWatch.Elapsed.TotalMilliseconds;
        var response = new BenchmarkResponse("Hash", operations, putWatch.Elapsed.TotalMilliseconds, getWatch.Elapsed.TotalMilliseconds, deleteWatch.Elapsed.TotalMilliseconds, totalMs);
        return Task.FromResult(Results.Ok(response));
    }
}
