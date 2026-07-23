using System.Diagnostics;
using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Core.Services;

public class BenchmarkService : IBenchmarkService
{
    public BenchmarkResponse Run(ISymbolTable<string, string> table, int operations, string structure, Func<StructureSnapshot> snapshotFactory)
    {
        var keys = new string[operations];
        for (int index = 0; index < operations; index++)
            keys[index] = CodeGenerator.Generate();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        var putWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.Put(keys[index], keys[index]);
        putWatch.Stop();

        var snapshot = snapshotFactory();

        var getWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.TryGet(keys[index], out _);
        getWatch.Stop();

        var deleteWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.Delete(keys[index]);
        deleteWatch.Stop();

        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        var totalMs = putWatch.Elapsed.TotalMilliseconds + getWatch.Elapsed.TotalMilliseconds + deleteWatch.Elapsed.TotalMilliseconds;

        return new BenchmarkResponse(
            structure,
            operations,
            snapshot.ElementCount,
            putWatch.Elapsed.TotalMilliseconds,
            getWatch.Elapsed.TotalMilliseconds,
            deleteWatch.Elapsed.TotalMilliseconds,
            totalMs,
            allocatedAfter - allocatedBefore,
            snapshot.TreeHeight,
            snapshot.BucketCount,
            snapshot.LoadFactor,
            snapshot.MaxChainLength
        );
    }
}
