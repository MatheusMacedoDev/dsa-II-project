using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Hash;

public class HashSymbolTableAlternative<TKey, TValue> : ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    private readonly List<LinkedList<KeyValuePair<TKey, TValue>>> _table;

    public HashSymbolTableAlternative(int capacity)
    {
        _table = new List<LinkedList<KeyValuePair<TKey, TValue>>>(capacity);
        for (var i = 0; i < _table.Capacity; i++)
            _table.Add(new LinkedList<KeyValuePair<TKey, TValue>>());
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
        if (list.Any(n => n.Key.Equals(key)))
        {
            throw new InvalidOperationException("Key already exists");
        }
        var node = new KeyValuePair<TKey, TValue>(key, value);
        list.AddLast(node);
    }

    public bool TryGet(TKey key, out TValue value)
    {
        var list = LinkedListFor(key);
        foreach (var node in list)
        {
            if (node.Key.Equals(key))
            {
                value = node.Value;
                return true;
            }
        }
        value = default!;
        return false;
    }

    public bool Delete(TKey key)
    {
        var list = LinkedListFor(key);
        try
        {
            var node = list.Single(n => n.Key.Equals(key));
            list.Remove(node);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool Contains(TKey key)
    {
        var list = LinkedListFor(key);
        return list.Any(n => n.Key.Equals(key));
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> Items()
    {
        foreach (var list in _table)
        {
            foreach (var node in list)
                yield return node;
        }
    }

    private LinkedList<KeyValuePair<TKey, TValue>> LinkedListFor(TKey key)
    {
        var hash = EqualityComparer<TKey>.Default.GetHashCode(key);
        var position = Math.Abs(hash % _table.Capacity);
        return _table[position];
    }
}

