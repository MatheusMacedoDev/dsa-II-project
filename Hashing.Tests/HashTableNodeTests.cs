namespace Hashing.Tests;

public class HashTableNodeTests
{
    [Test]
    public void Constructor_SetsKeyAndValue()
    {
        var node = new HashTableNode<string>(1, "test");

        Assert.That(node.key, Is.EqualTo(1));
        Assert.That(node.value, Is.EqualTo("test"));
    }

    [Test]
    public void NodesWithSameKeyAndValue_AreEqual()
    {
        var a = new HashTableNode<int>(42, 100);
        var b = new HashTableNode<int>(42, 100);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void NodesWithDifferentKey_AreNotEqual()
    {
        var a = new HashTableNode<int>(1, 100);
        var b = new HashTableNode<int>(2, 100);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void NodesWithDifferentValue_AreNotEqual()
    {
        var a = new HashTableNode<int>(1, 100);
        var b = new HashTableNode<int>(1, 200);

        Assert.That(a, Is.Not.EqualTo(b));
    }
}
