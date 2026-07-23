using EncurtadorUfabc.Hash;

namespace EncurtadorUfabc.Tests;

public class HashSymbolTableAlternativeTests
{
    private HashSymbolTableAlternative<int, string> Create(int capacity = 17) => new(capacity);

    [Fact]
    public void Put_NewKey_IncreasesCount()
    {
        var table = Create();
        table.Put(1, "one");
        table.Put(2, "two");
        Assert.Equal(2, table.Count);
    }

    [Fact]
    public void Put_DuplicateKey_Throws()
    {
        var table = Create();
        table.Put(42, "x");
        Assert.Throws<InvalidOperationException>(() => table.Put(42, "y"));
    }

    [Fact]
    public void TryGet_ExistingKey_Succeeds()
    {
        var table = Create();
        table.Put(7, "seven");
        Assert.True(table.TryGet(7, out var value));
        Assert.Equal("seven", value);
    }

    [Fact]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        var table = Create();
        Assert.False(table.TryGet(99, out _));
    }

    [Fact]
    public void Contains_Works()
    {
        var table = Create();
        table.Put(10, "ten");
        Assert.True(table.Contains(10));
        Assert.False(table.Contains(11));
    }

    [Fact]
    public void Delete_ExistingKey_RemovesIt()
    {
        var table = Create();
        table.Put(1, "one");
        table.Put(2, "two");
        Assert.True(table.Delete(2));
        Assert.False(table.Contains(2));
        Assert.Equal(1, table.Count);
    }

    [Fact]
    public void Delete_MissingKey_ReturnsFalse()
    {
        var table = Create();
        table.Put(1, "one");
        Assert.False(table.Delete(99));
        Assert.Equal(1, table.Count);
    }

    [Fact]
    public void Items_ReturnsAllPairs()
    {
        var table = Create();
        table.Put(5, "five");
        table.Put(2, "two");
        table.Put(7, "seven");
        var items = table.Items().OrderBy(kv => kv.Key).ToArray();
        Assert.Equal([2, 5, 7], items.Select(kv => kv.Key));
        Assert.Equal(["two", "five", "seven"], items.Select(kv => kv.Value));
    }

    [Fact]
    public void ManyInsertions_PreserveAllAssociations()
    {
        var table = Create(101);
        for (int i = 0; i < 1000; i++)
            table.Put(i, $"val-{i}");
        Assert.Equal(1000, table.Count);

        // spot check
        Assert.True(table.TryGet(123, out var v123));
        Assert.Equal("val-123", v123);
        Assert.True(table.Contains(900));
        Assert.True(table.Delete(0));
        Assert.False(table.Contains(0));
    }
}
