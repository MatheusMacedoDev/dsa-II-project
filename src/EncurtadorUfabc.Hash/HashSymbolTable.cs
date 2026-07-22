using EncurtadorUfabc.Core.Models;

namespace EncurtadorUfabc.Hash;

// Tabela de simbolos implementada como tabela de dispersao (hash table) com
// tratamento de colisoes por Encadeamento EXTERNO no qual cada
// posicao do vetor de registros referencia uma lista ligada simples de
// entradas cujas chaves colidiram na mesma posicao apos a compressao.
public class HashSymbolTable<TKey, TValue> : ISymbolTable<TKey, TValue> where TKey : IComparable<TKey>
{
    // No da lista ligada de encadeamento externo. Cada balde e a cabeca de uma
    // dessas listas, entradas que colidem no mesmo indice sao encadeadas via Next.
    private sealed class HashEntry
    {
        public TKey Key;
        public TValue Value;
        public HashEntry? Next;

        public HashEntry(TKey key, TValue value, HashEntry? next)
        {
            Key = key;
            Value = value;
            Next = next;
        }
    }

    // Sequencia de tamanhos primos usada a cada redimensionamento. Tamanhos primos
    // foram escolhidos na etapa de compressao por divisao, pois reduzem padroes de
    // colisao.
    private static readonly int[] PrimeSizes = { 7, 17, 37, 79, 163, 331, 673, 1361, 2729, 5471, 10949, 21911, 43853, 87719, 175447, 350899 };

    private HashEntry?[] buckets;
    private int primeSizeIndex;
    private int count;
    private const double LoadFactorThreshold = 0.75;

    public HashSymbolTable()
    {
        primeSizeIndex = 0;
        buckets = new HashEntry?[PrimeSizes[primeSizeIndex]];
        count = 0;
    }

    public int Count => count;

    // Etapa 1 da funcao de dispersao: transforma a chave em um numero inteiro
    // (codigo de dispersao). Para chaves do tipo string calcula-se um hash
    // polinomial pela regra de Horner (base 31), percorrendo os caracteres pelo
    // indexador, sem gerar substrings nem alocacoes intermediarias. Para os demais
    // tipos delega-se ao GetHashCode padrao do proprio tipo da chave.
    private static int ComputeHashCode(TKey key)
    {
        if (key is string text)
        {
            int hashCode = 0;
            for (int index = 0; index < text.Length; index++)
                hashCode = 31 * hashCode + text[index];
            return hashCode;
        }

        return key!.GetHashCode();
    }

    // Etapa 2 da funcao de dispersao: compressao por divisao. Mascara-se o bit de
    // sinal para garantir um valor nao negativo e aplica-se o resto da divisao pelo
    // tamanho (primo) do vetor, obtendo um indice valido em [0, tableSize - 1].
    private static int CompressToIndex(int hashCode, int tableSize) => (hashCode & 0x7FFFFFFF) % tableSize;

    private int IndexFor(TKey key) => CompressToIndex(ComputeHashCode(key), buckets.Length);

    private static bool KeysAreEqual(TKey first, TKey second) => first.CompareTo(second) == 0;

    // Insere ou atualiza a associacao chave-valor. Se a chave ja existir na cadeia
    // do balde, apenas o valor e substituido, caso contrario uma nova entrada e
    // inserida no inicio da lista ligada. Ao ultrapassar o fator de carga limite a
    // tabela e redimensionada e as entradas sao redistribuidas (rehashing).
    public void Put(TKey key, TValue value)
    {
        int bucketIndex = IndexFor(key);

        for (HashEntry? entry = buckets[bucketIndex]; entry is not null; entry = entry.Next)
        {
            if (KeysAreEqual(entry.Key, key))
            {
                entry.Value = value;
                return;
            }
        }

        buckets[bucketIndex] = new HashEntry(key, value, buckets[bucketIndex]);
        count++;

        if (CurrentLoadFactor() > LoadFactorThreshold)
            Resize();
    }

    public bool TryGet(TKey key, out TValue value)
    {
        int bucketIndex = IndexFor(key);

        for (HashEntry? entry = buckets[bucketIndex]; entry is not null; entry = entry.Next)
        {
            if (KeysAreEqual(entry.Key, key))
            {
                value = entry.Value;
                return true;
            }
        }

        value = default!;
        return false;
    }

    // Remove a associacao da chave informada percorrendo a cadeia do balde e
    // religando o predecessor ao sucessor da entrada removida.
    public bool Delete(TKey key)
    {
        int bucketIndex = IndexFor(key);
        HashEntry? previous = null;

        for (HashEntry? entry = buckets[bucketIndex]; entry is not null; entry = entry.Next)
        {
            if (KeysAreEqual(entry.Key, key))
            {
                if (previous is null)
                    buckets[bucketIndex] = entry.Next;
                else
                    previous.Next = entry.Next;

                count--;
                return true;
            }

            previous = entry;
        }

        return false;
    }

    public bool Contains(TKey key) => TryGet(key, out _);

    // Percorre todos os baldes e suas respectivas cadeias, produzindo cada
    // associacao chave-valor. A ordem de iteracao segue a distribuicao interna da
    // tabela e nao possui garantia de ordenacao.
    public IEnumerable<KeyValuePair<TKey, TValue>> Items()
    {
        foreach (HashEntry? bucket in buckets)
        {
            for (HashEntry? entry = bucket; entry is not null; entry = entry.Next)
                yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }

    // Fator de carga: razao entre o numero de entradas armazenadas e o numero de
    // baldes. Quanto maior, mais longas tendem a ficar as cadeias, degradando o
    // custo esperado das operacoes de O(1) amortizado para O(n) no pior caso.
    private double CurrentLoadFactor() => (double)count / buckets.Length;

    // Redimensiona a tabela para o proximo tamanho primo disponivel e reinsere
    // (rehashing) todas as entradas, recalculando o indice de cada uma em funcao do
    // novo tamanho, de modo a restaurar cadeias curtas e o desempenho esperado.
    private void Resize()
    {
        if (primeSizeIndex >= PrimeSizes.Length - 1)
            return;

        HashEntry?[] previousBuckets = buckets;
        primeSizeIndex++;
        buckets = new HashEntry?[PrimeSizes[primeSizeIndex]];

        foreach (HashEntry? bucket in previousBuckets)
        {
            for (HashEntry? entry = bucket; entry is not null; entry = entry.Next)
            {
                int bucketIndex = IndexFor(entry.Key);
                buckets[bucketIndex] = new HashEntry(entry.Key, entry.Value, buckets[bucketIndex]);
            }
        }
    }
}
