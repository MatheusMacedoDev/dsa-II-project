using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Core.Services;

public interface IBenchmarkService
{
    BenchmarkResponse Run(ISymbolTable<string, string> table, int operations, string structure, Func<StructureSnapshot> snapshotFactory);
}
