using System.Diagnostics;
using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Core.Services;

public class BenchmarkService : IBenchmarkService
{
    public BenchmarkResponse Run(ISymbolTable<string, string> table, int operations, string structure)
    {
        var keys = new string[operations];
        for (int index = 0; index < operations; index++)
            keys[index] = $"benchmark-key-{index}";

        var putWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.Put(keys[index], keys[index]);
        putWatch.Stop();

        var getWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.TryGet(keys[index], out _);
        getWatch.Stop();

        var deleteWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.Delete(keys[index]);
        deleteWatch.Stop();

        var totalMs = putWatch.Elapsed.TotalMilliseconds
            + getWatch.Elapsed.TotalMilliseconds
            + deleteWatch.Elapsed.TotalMilliseconds;

        return new BenchmarkResponse(
            structure,
            operations,
            putWatch.Elapsed.TotalMilliseconds,
            getWatch.Elapsed.TotalMilliseconds,
            deleteWatch.Elapsed.TotalMilliseconds,
            totalMs
        );
    }
}
