namespace EncurtadorUfabc.AVL;

public class AvlNode<TKey, TValue> where TKey : IComparable<TKey>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public int Height { get; set; }
    public AvlNode<TKey, TValue>? Left { get; set; }
    public AvlNode<TKey, TValue>? Right { get; set; }

    public AvlNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        Height = 1;
    }
}
