using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Core.Services;

public interface IBenchmarkService
{
    BenchmarkResponse Run(
        Func<ISymbolTable<string, string>> tableFactory,
        int operations,
        string structure,
        Func<ISymbolTable<string, string>, StructureSnapshot> snapshotFactory
    );
}
