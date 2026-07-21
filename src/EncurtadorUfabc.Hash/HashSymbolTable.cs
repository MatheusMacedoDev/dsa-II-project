using EncurtadorUfabc.Core.Contracts;

namespace EncurtadorUfabc.Hash;

public class HashSymbolTable<TKey, TValue> : ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    public int Count => throw new NotImplementedException();

    public void Put(TKey key, TValue value) => throw new NotImplementedException();

    public bool TryGet(TKey key, out TValue value) => throw new NotImplementedException();

    public bool Delete(TKey key) => throw new NotImplementedException();

    public bool Contains(TKey key) => throw new NotImplementedException();

    public IEnumerable<KeyValuePair<TKey, TValue>> Items() => throw new NotImplementedException();
}
