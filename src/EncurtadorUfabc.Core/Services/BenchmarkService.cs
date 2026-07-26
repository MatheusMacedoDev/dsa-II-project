using System.Diagnostics;
using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Core.Services;

public class BenchmarkService : IBenchmarkService
{
    private const int WarmupOperations = 1000;

    public BenchmarkResponse Run(
        Func<ISymbolTable<string, string>> tableFactory,
        int operations,
        string structure,
        Func<ISymbolTable<string, string>, StructureSnapshot> snapshotFactory
    )
    {
        Warmup(tableFactory);

        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true);
        GC.WaitForPendingFinalizers();

        var keys = new string[operations];
        for (int index = 0; index < operations; index++)
            keys[index] = CodeGenerator.Generate();

        var table = tableFactory();
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        var putWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.Put(keys[index], keys[index]);
        putWatch.Stop();

        var snapshot = snapshotFactory(table);

        var getWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.TryGet(keys[index], out _);
        getWatch.Stop();

        var deleteWatch = Stopwatch.StartNew();
        for (int index = 0; index < operations; index++)
            table.Delete(keys[index]);
        deleteWatch.Stop();

        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

        var putMs = Math.Round(putWatch.Elapsed.TotalMilliseconds, 4);
        var getMs = Math.Round(getWatch.Elapsed.TotalMilliseconds, 4);
        var deleteMs = Math.Round(deleteWatch.Elapsed.TotalMilliseconds, 4);
        var totalMs = Math.Round(putMs + getMs + deleteMs, 4);

        return new BenchmarkResponse(
            structure,
            operations,
            snapshot.ElementCount,
            putMs,
            getMs,
            deleteMs,
            totalMs,
            allocatedAfter - allocatedBefore,
            snapshot.TreeHeight,
            snapshot.BucketCount,
            snapshot.LoadFactor,
            snapshot.MaxChainLength
        );
    }

    private static void Warmup(Func<ISymbolTable<string, string>> tableFactory)
    {
        var table = tableFactory();
        var keys = new string[WarmupOperations];
        for (int index = 0; index < WarmupOperations; index++)
            keys[index] = CodeGenerator.Generate();

        for (int index = 0; index < WarmupOperations; index++)
            table.Put(keys[index], keys[index]);

        for (int index = 0; index < WarmupOperations; index++)
            table.TryGet(keys[index], out _);

        for (int index = 0; index < WarmupOperations; index++)
            table.Delete(keys[index]);
    }
}
