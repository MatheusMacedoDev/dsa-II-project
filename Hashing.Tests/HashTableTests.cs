namespace Hashing.Tests;

public class HashTableTests
{
    private HashTable<string> _table = null!;

    [SetUp]
    public void Setup()
    {
        _table = new HashTable<string>(20);
    }

    [Test]
    public void Add_ThenGet_ReturnsSameValue()
    {
        _table.Add(5, "hello");

        var result = _table.Get(5);

        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public void Add_MultipleItems_RetrieveCorrectly()
    {
        _table.Add(3, "three");
        _table.Add(7, "seven");
        _table.Add(15, "fifteen");

        Assert.That(_table.Get(3), Is.EqualTo("three"));
        Assert.That(_table.Get(7), Is.EqualTo("seven"));
        Assert.That(_table.Get(15), Is.EqualTo("fifteen"));
    }

    [Test]
    public void Add_DuplicateKey_ThrowsArgumentException()
    {
        _table.Add(10, "first");

        Assert.That(() => _table.Add(10, "second"), Throws.ArgumentException);
    }

    [Test]
    public void Add_Collision_HandlesCorrectly()
    {
        _table.Add(0, "zero");
        _table.Add(20, "twenty");
        _table.Add(40, "forty");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_table.Get(0), Is.EqualTo("zero"));
            Assert.That(_table.Get(20), Is.EqualTo("twenty"));
            Assert.That(_table.Get(40), Is.EqualTo("forty"));
        }
    }

    [Test]
    public void Get_NonExistentKey_Throws()
    {
        Assert.That(() => _table.Get(999), Throws.Exception);
    }
}
