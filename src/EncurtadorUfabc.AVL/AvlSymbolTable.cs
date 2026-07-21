using EncurtadorUfabc.Core.Contracts;

namespace EncurtadorUfabc.AVL;

public class AvlSymbolTable<TKey, TValue> : ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    private AvlNode<TKey, TValue>? _root;

    public int Count => throw new NotImplementedException();

    public void Put(TKey key, TValue value) => throw new NotImplementedException();

    public bool TryGet(TKey key, out TValue value) => throw new NotImplementedException();

    public bool Delete(TKey key) => throw new NotImplementedException();

    public bool Contains(TKey key) => throw new NotImplementedException();

    public IEnumerable<KeyValuePair<TKey, TValue>> Items() => throw new NotImplementedException();
}
