namespace EncurtadorUfabc.Core.Contracts;

public interface ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    int Count { get; }
    void Put(TKey key, TValue value);
    bool TryGet(TKey key, out TValue value);
    bool Delete(TKey key);
    bool Contains(TKey key);
    IEnumerable<KeyValuePair<TKey, TValue>> Items();
}
