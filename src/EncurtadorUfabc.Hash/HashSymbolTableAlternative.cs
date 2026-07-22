using EncurtadorUfabc.Core.Contracts;

namespace EncurtadorUfabc.Hash;

public class HashSymbolTableAlternative<TKey, TValue> : ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    private readonly List<LinkedList<HashTableNode<TValue>>> _table;

    public HashSymbolTableAlternative(int capacity)
    {
        _table = new List<LinkedList<HashTableNode<TValue>>>(capacity);
        for (var i = 0; i < _table.Capacity; i++)
            _table.Add(new LinkedList<HashTableNode<TValue>>());
    }

    public int Count
    {
        get
        {
            int result = 0;
            foreach (var list in _table)
                result += list.Count;
            return result;
        }
    }

    public void Put(TKey key, TValue value)
    {
        var list = LinkedListFor(key);
        if (list.Any(n => n.key == (int)(object)key))
        {
            throw new InvalidOperationException("Key already exists");
        }
        var node = new HashTableNode<TValue>((int)(object)key, value);
        list.AddLast(node);
    }

    public bool TryGet(TKey key, out TValue value)
    {
        var list = LinkedListFor(key);
        foreach (var node in list)
        {
            if (node.key == (int)(object)key)
            {
                value = node.value;
                return true;
            }
        }
        value = default!;
        return false;
    }

    public bool Delete(TKey key)
    {
        var list = LinkedListFor(key);
        var node = list.FirstOrDefault(n => n.key == key);
        if (node == null) return false;
        list.Remove(node);
        return true;
    }

    public bool Contains(TKey key)
    {
        var list = LinkedListFor(key);
        return list.Any(n => n.key == key);
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> Items()
    {
        foreach (var list in _table)
        {
            foreach (var node in list)
                yield return new KeyValuePair<TKey, TValue>(node.key, node.value);
        }
    }

    private LinkedList<HashTableNode<TValue>> LinkedListFor(TKey key)
    {
        var hash = EqualityComparer<TKey>.Default.GetHashCode(key);
        var position = Math.Abs(hash % _table.Capacity);
        return _table[position];
    }
}

