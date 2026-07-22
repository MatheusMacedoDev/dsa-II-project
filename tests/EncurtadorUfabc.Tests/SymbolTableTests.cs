using EncurtadorUfabc.AVL;
using EncurtadorUfabc.Core.Models;
using EncurtadorUfabc.Hash;

namespace EncurtadorUfabc.Tests;

public abstract class SymbolTableTests
{
    protected abstract ISymbolTable<string, int> Create();

    [Fact]
    public void Put_NewKey_IncreasesCount()
    {
        var table = Create();

        table.Put("a", 1);
        table.Put("b", 2);

        Assert.Equal(2, table.Count);
    }

    [Fact]
    public void Put_ExistingKey_UpdatesValueWithoutIncreasingCount()
    {
        var table = Create();

        table.Put("a", 1);
        table.Put("a", 99);

        Assert.Equal(1, table.Count);
        Assert.True(table.TryGet("a", out int value));
        Assert.Equal(99, value);
    }

    [Fact]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        var table = Create();

        Assert.False(table.TryGet("missing", out _));
    }

    [Fact]
    public void Contains_ReflectsPresence()
    {
        var table = Create();
        table.Put("a", 1);

        Assert.True(table.Contains("a"));
        Assert.False(table.Contains("b"));
    }

    [Fact]
    public void Delete_ExistingKey_RemovesIt()
    {
        var table = Create();
        table.Put("a", 1);
        table.Put("b", 2);

        Assert.True(table.Delete("a"));
        Assert.False(table.Contains("a"));
        Assert.True(table.Contains("b"));
        Assert.Equal(1, table.Count);
    }

    [Fact]
    public void Delete_MissingKey_ReturnsFalse()
    {
        var table = Create();
        table.Put("a", 1);

        Assert.False(table.Delete("missing"));
        Assert.Equal(1, table.Count);
    }

    [Fact]
    public void Items_ReturnsAllStoredPairs()
    {
        var table = Create();
        table.Put("a", 1);
        table.Put("b", 2);
        table.Put("c", 3);

        var items = table.Items().OrderBy(pair => pair.Key).ToArray();

        Assert.Equal(new[] { "a", "b", "c" }, items.Select(pair => pair.Key));
        Assert.Equal(new[] { 1, 2, 3 }, items.Select(pair => pair.Value));
    }

    [Fact]
    public void ManyInsertions_PreserveAllAssociations()
    {
        var table = Create();

        for (int index = 0; index < 1000; index++)
            table.Put($"key-{index}", index);

        Assert.Equal(1000, table.Count);

        for (int index = 0; index < 1000; index++)
        {
            Assert.True(table.TryGet($"key-{index}", out int value));
            Assert.Equal(index, value);
        }
    }
}

public class HashSymbolTableTests : SymbolTableTests
{
    protected override ISymbolTable<string, int> Create() => new HashSymbolTable<string, int>();
}

public class AvlSymbolTableTests : SymbolTableTests
{
    protected override ISymbolTable<string, int> Create() => new AvlSymbolTable<string, int>();

    [Fact]
    public void Items_AreReturnedInAscendingKeyOrder()
    {
        var table = new AvlSymbolTable<string, int>();
        table.Put("c", 3);
        table.Put("a", 1);
        table.Put("b", 2);

        Assert.Equal(new[] { "a", "b", "c" }, table.Items().Select(pair => pair.Key));
    }
}
