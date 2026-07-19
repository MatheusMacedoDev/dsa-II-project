namespace Hashing;

public class HashTable<TValue>
{
    private List<LinkedList<HashTableNode<TValue>>> _table;

    public HashTable(int capacity)
    {
        _table = new List<LinkedList<HashTableNode<TValue>>>(capacity);
        for (var i =  0; i < _table.Capacity; i++)
            _table.Add(new LinkedList<HashTableNode<TValue>>());
    }

    public void Add(int key, TValue value)
    {
        var position = key % _table.Capacity;
        var list = _table[position];
        if (list.Any(n => n.key == key))
        {
            throw new ArgumentException("Key already exists");
        }
        var node = new HashTableNode<TValue>(key, value);
        list.AddLast(node);
    }

    public TValue? Get(int key)
    {
        var position = key % _table.Capacity;
        var list = _table[position];
        return list.Single(n => n.key == key).value;
    }
}
