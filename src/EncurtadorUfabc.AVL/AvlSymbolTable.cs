using EncurtadorUfabc.Core.Contracts;

namespace EncurtadorUfabc.AVL;

// Tabela de simbolos implementada como arvore AVL: uma arvore binaria de busca
// auto-balanceada. A invariante de balanceamento exige que, para todo no, o
// fator de balanceamento fique no intervalo {-1, 0, +1}. Manter essa invariante garante altura
// logaritmica e alcançamos custo O(log n) nas operacoes de busca, insercao e remocao.
public class AvlSymbolTable<TKey, TValue> : ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    private AvlNode<TKey, TValue>? root;
    private int count;

    public int Count => count;

    // Altura de um no, tratando a subarvore vazia (null) como altura zero.
    private static int GetHeight(AvlNode<TKey, TValue>? node) => node is null ? 0 : node.Height;

    // Recalcula a altura de um no a partir das alturas de seus filhos.
    private static void UpdateHeight(AvlNode<TKey, TValue> node) => node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

    // Fator de balanceamento: diferenca entre as alturas das subarvores esquerda e direita. Valores fora de {-1, 0, +1} indicam desequilibrio a ser corrigido.

    private static int BalanceFactor(AvlNode<TKey, TValue> node) => GetHeight(node.Left) - GetHeight(node.Right);

    // Rotacao simples a direita, usada para corrigir desequilibrios do lado
    // esquerdo. O filho esquerdo sobe e passa a ser a raiz local da subarvore.
    private static AvlNode<TKey, TValue> RotateRight(AvlNode<TKey, TValue> node)
    {
        AvlNode<TKey, TValue> newRoot = node.Left!;
        node.Left = newRoot.Right;
        newRoot.Right = node;

        UpdateHeight(node);
        UpdateHeight(newRoot);
        return newRoot;
    }

    // Rotacao simples a esquerda, usada para corrigir desequilibrios do lado
    // direito. O filho direito sobe e passa a ser a raiz local da subarvore.
    private static AvlNode<TKey, TValue> RotateLeft(AvlNode<TKey, TValue> node)
    {
        AvlNode<TKey, TValue> newRoot = node.Right!;
        node.Right = newRoot.Left;
        newRoot.Left = node;

        UpdateHeight(node);
        UpdateHeight(newRoot);
        return newRoot;
    }

    // Rebalanceia o no apos uma insercao ou remocao, aplicando a rotacao correta
    // conforme o caso de desequilibrio que foi detectado: Esquerda-Esquerda (EE),
    // Esquerda-Direita (ED), Direita-Direita (DD) ou Direita-Esquerda (DE).
    private static AvlNode<TKey, TValue> Balance(AvlNode<TKey, TValue> node)
    {
        UpdateHeight(node);
        int balance = BalanceFactor(node);

        if (balance > 1)
        {
            if (BalanceFactor(node.Left!) < 0)
                node.Left = RotateLeft(node.Left!);
            return RotateRight(node);
        }

        if (balance < -1)
        {
            if (BalanceFactor(node.Right!) > 0)
                node.Right = RotateRight(node.Right!);
            return RotateLeft(node);
        }

        return node;
    }

    // Insere ou atualiza a associacao chave-valor.
    public void Put(TKey key, TValue value) => root = Insert(root, key, value);

    private AvlNode<TKey, TValue> Insert(AvlNode<TKey, TValue>? node, TKey key, TValue value)
    {
        if (node is null)
        {
            count++;
            return new AvlNode<TKey, TValue>(key, value);
        }

        int comparison = key.CompareTo(node.Key);

        if (comparison < 0)
            node.Left = Insert(node.Left, key, value);
        else if (comparison > 0)
            node.Right = Insert(node.Right, key, value);
        else
        {
            node.Value = value;
            return node;
        }

        return Balance(node);
    }

    // Busca a chave descendo pela arvore conforme a comparacao (busca binaria),
    // retornando o valor associado quando encontrada.
    public bool TryGet(TKey key, out TValue value)
    {
        AvlNode<TKey, TValue>? current = root;

        while (current is not null)
        {
            int comparison = key.CompareTo(current.Key);

            if (comparison < 0)
                current = current.Left;
            else if (comparison > 0)
                current = current.Right;
            else
            {
                value = current.Value;
                return true;
            }
        }

        value = default!;
        return false;
    }

    // Remove a associacao da chave informada. Apos a remocao, os nos
    // no caminho de volta sao rebalanceados para manter a AVL.
    public bool Delete(TKey key)
    {
        int previousCount = count;
        root = Remove(root, key);
        return count < previousCount;
    }

    private AvlNode<TKey, TValue>? Remove(AvlNode<TKey, TValue>? node, TKey key)
    {
        if (node is null)
            return null;

        int comparison = key.CompareTo(node.Key);

        if (comparison < 0)
            node.Left = Remove(node.Left, key);
        else if (comparison > 0)
            node.Right = Remove(node.Right, key);
        else
        {
            count--;

            if (node.Left is null)
                return node.Right;
            if (node.Right is null)
                return node.Left;

            // No com dois filhos: substitui o conteudo pelo sucessor
            // com menor chave da subarvore direita e remove esse sucessor.
            AvlNode<TKey, TValue> successor = FindMinimum(node.Right);
            node.Key = successor.Key;
            node.Value = successor.Value;
            node.Right = RemoveMinimum(node.Right);
        }

        return Balance(node);
    }

    // Localiza o no de menor chave de uma subarvore, seguindo sempre a esquerda.
    private static AvlNode<TKey, TValue> FindMinimum(AvlNode<TKey, TValue> node)
    {
        AvlNode<TKey, TValue> current = node;
        while (current.Left is not null)
            current = current.Left;
        return current;
    }

    // Remove o no de menor chave de uma subarvore, rebalanceando no retorno.
    private static AvlNode<TKey, TValue>? RemoveMinimum(AvlNode<TKey, TValue> node)
    {
        if (node.Left is null)
            return node.Right;

        node.Left = RemoveMinimum(node.Left);
        return Balance(node);
    }

    public bool Contains(TKey key) => TryGet(key, out _);

    // Percurso em-ordem: visita subarvore esquerda, no e subarvore
    // direita, produzindo as associacoes em ordem crescente de chave.
    public IEnumerable<KeyValuePair<TKey, TValue>> Items() => TraverseInOrder(root);

    private static IEnumerable<KeyValuePair<TKey, TValue>> TraverseInOrder(AvlNode<TKey, TValue>? node)
    {
        if (node is null)
            yield break;

        foreach (KeyValuePair<TKey, TValue> item in TraverseInOrder(node.Left))
            yield return item;

        yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);

        foreach (KeyValuePair<TKey, TValue> item in TraverseInOrder(node.Right))
            yield return item;
    }
}
