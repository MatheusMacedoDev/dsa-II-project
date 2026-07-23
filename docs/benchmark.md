# Metodologia do Benchmark

## Visão geral

O benchmark do EncurtadorUfabc compara o desempenho de duas implementações da interface `ISymbolTable<TKey, TValue>`: a **Árvore AVL** e a **Tabela Hash**. O objetivo é medir e contrastar o custo de tempo e de alocação de memória das operações fundamentais de uma tabela de símbolos — inserção, busca e remoção — em cada uma das estruturas.

Os dados coletados permitem observar não apenas *qual* estrutura é mais rápida para uma determinada carga de trabalho, mas também *por quê*, expondo informações sobre o estado interno de cada implementação após a população completa da tabela.

## Metodologia

### Fluxo de execução

Cada execução do benchmark segue rigorosamente a mesma sequência de fases, executadas sobre uma instância **nova** da estrutura selecionada (nenhum dado pré-existente):

1. **Geração de chaves**: um vetor de `N` chaves do tipo `string` é gerado no formato `"benchmark-key-0"`, `"benchmark-key-1"`, ..., `"benchmark-key-N-1"`. As chaves são strings sequenciais e previsíveis, o que favorece a Tabela Hash (boa distribuição pelo hash polinomial) e a Árvore AVL (ordem natural crescente, que exercita o pior caso de desbalanceamento e, portanto, as rotações).

2. **Fase de inserção (Put)**: as `N` chaves são inseridas uma a uma, com o valor sendo a própria chave. O tempo total da fase é medido com `Stopwatch`.

3. **Snapshot estrutural**: imediatamente após a inserção, os seguintes dados são capturados da estrutura populada:
   - Quantidade de elementos armazenados (`elementCount`)
   - Para a Árvore AVL: altura da árvore (`treeHeight`)
   - Para a Tabela Hash: tamanho do vetor de baldes (`bucketCount`), fator de carga (`loadFactor`) e comprimento da maior cadeia de colisão (`maxChainLength`)

4. **Fase de busca (Get)**: as `N` chaves são consultadas uma a uma via `TryGet`, no exato oposto da ordem de inserção (do último índice ao primeiro), para evitar favorecimento de cache. O tempo total da fase é medido com `Stopwatch`.

5. **Fase de remoção (Delete)**: as `N` chaves são removidas uma a uma, na mesma ordem em que foram inseridas. O tempo total da fase é medido com `Stopwatch`.

### Medição de memória

A medição de alocações de memória utiliza o método `GC.GetAllocatedBytesForCurrentThread()`, disponível a partir do .NET 6. Uma leitura de referência é feita **antes** da geração das chaves e uma leitura final é feita **após** a última remoção. A diferença entre as duas leituras (`allocatedBytes`) representa o total de bytes alocados no heap gerenciado pela thread corrente durante toda a execução do benchmark.

Este valor captura:

- Na **Árvore AVL**: a criação de `AvlNode` para cada inserção, além de quaisquer alocações temporárias durante rotações e rebalanceamentos.
- Na **Tabela Hash**: a criação de `HashEntry` para cada inserção e a realocação completa do vetor de baldes durante cada evento de redimensionamento (`Resize`).

As fases de busca e remoção **não alocam** novos objetos em nenhuma das duas estruturas (apenas percorrem e desligam referências), portanto a quase totalidade do `allocatedBytes` provém da fase de inserção.

### Medição de tempo

Cada fase é medida com `System.Diagnostics.Stopwatch`, que utiliza o contador de alta precisão do sistema operacional (`QueryPerformanceCounter` no Windows, `clock_gettime` no Linux). O tempo é reportado em milissegundos com precisão de submilissegundos (via `Elapsed.TotalMilliseconds`).

**Não é realizado aquecimento da JIT (Just-In-Time compilation)** antes das medições. Isso significa que as primeiras chamadas de método podem incluir o custo de compilação JIT, o que é consistente entre as duas estruturas (ambas sofrem o mesmo efeito) e representa o comportamento real de uma primeira execução.

## Estruturas comparadas

### Árvore AVL (`AvlSymbolTable<TKey, TValue>`)

Árvore binária de busca auto-balanceada. A invariante de balanceamento exige que, para todo nó, o fator de balanceamento (diferença entre as alturas das subárvores esquerda e direita) esteja no intervalo `{-1, 0, +1}`. Rotações à esquerda e à direita são aplicadas após cada inserção e remoção para restaurar o balanceamento.

- **Inserção**: O(log n) — descida binária até a posição correta, seguida de rebalanceamento no caminho de volta.
- **Busca**: O(log n) — descida binária pela árvore.
- **Remoção**: O(log n) — descida binária, substituição pelo sucessor (quando há dois filhos) e rebalanceamento no caminho de volta.
- **Complexidade de espaço**: O(n) — um nó (`AvlNode`) por elemento, cada nó armazena chave, valor, altura e duas referências (esquerda/direita).

A altura de uma árvore AVL com `n` nós está no intervalo `[⌊log₂(n+1)⌋, 1.44 × log₂(n)]`. A altura observada (`treeHeight`) indica a qualidade do balanceamento: valores próximos ao limite superior sugerem que a ordem de inserção foi adversa e forçou muitas rotações.

### Tabela Hash (`HashSymbolTable<TKey, TValue>`)

Tabela de dispersão com tratamento de colisões por **encadeamento externo** (separate chaining). Cada posição do vetor de baldes referencia uma lista ligada simples de entradas cujas chaves colidiram na mesma posição após a função de dispersão.

- **Função de dispersão**: hash polinomial (regra de Horner, base 31) para chaves `string`; `GetHashCode()` padrão para os demais tipos.
- **Compressão**: método da divisão com tamanho de tabela primo.
- **Redimensionamento**: quando o fator de carga (`loadFactor`) ultrapassa **0,75**, a tabela é redimensionada para o próximo tamanho primo da sequência `{7, 17, 37, 79, ..., 350899}` e todas as entradas são redistribuídas (rehashing).
- **Inserção**: O(1) amortizado — hash, compressão e inserção no início da lista ligada do balde.
- **Busca**: O(1) amortizado, O(n) no pior caso — hash e percurso da cadeia do balde.
- **Remoção**: O(1) amortizado, O(n) no pior caso — busca na cadeia e religação do predecessor.
- **Complexidade de espaço**: O(n + m) — um nó (`HashEntry`) por elemento mais o vetor de `m` baldes.

O `maxChainLength` indica o pior caso de colisão: um valor muito maior que a média (`loadFactor`) sinaliza má distribuição das chaves pela função de hash ou um tamanho de tabela sub-ótimo.

## Campos da resposta

Cada execução do benchmark retorna um objeto JSON com os seguintes campos:

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `structure` | `string` | Identificador da estrutura: `"AVL"` ou `"Hash"`. |
| `operations` | `int` | Quantidade de operações executadas em cada fase (inserção, busca e remoção). |
| `elementCount` | `int` | Quantidade de elementos armazenados na estrutura após a fase de inserção. Deve ser igual a `operations`. |
| `putMs` | `double` | Tempo total da fase de inserção, em milissegundos. |
| `getMs` | `double` | Tempo total da fase de busca, em milissegundos. |
| `deleteMs` | `double` | Tempo total da fase de remoção, em milissegundos. |
| `totalMs` | `double` | Soma dos tempos das três fases (`putMs + getMs + deleteMs`). |
| `allocatedBytes` | `long` | Total de bytes alocados no heap gerenciado durante toda a execução do benchmark (da geração das chaves até a última remoção). |
| `treeHeight` | `int?` | Altura da árvore AVL após a inserção de todos os elementos. `null` para a Tabela Hash. |
| `bucketCount` | `int?` | Tamanho atual do vetor de baldes da Tabela Hash após a inserção. `null` para a Árvore AVL. |
| `loadFactor` | `double?` | Fator de carga da Tabela Hash (`elementCount / bucketCount`). `null` para a Árvore AVL. |
| `maxChainLength` | `int?` | Comprimento da maior cadeia de colisão na Tabela Hash. `null` para a Árvore AVL. |

## Como interpretar os resultados

### Comparação geral de desempenho

- **Inserção (`putMs`)**: a Tabela Hash tende a ser mais rápida que a AVL para inserções (O(1) amortizado vs. O(log n)). No entanto, se o benchmark for executado com uma quantidade de operações que dispare múltiplos redimensionamentos, a Hash pode apresentar picos de latência visíveis no `allocatedBytes`.
- **Busca (`getMs`)**: ambas são muito rápidas, mas a Hash com boas condições (baixo `loadFactor`, cadeias curtas) costuma superar a AVL. Se `maxChainLength` for alto, o desempenho da busca na Hash degrada para O(n) nos baldes mais populados.
- **Remoção (`deleteMs`)**: comportamento semelhante à busca, com a diferença de que a AVL precisa rebalancear no caminho de volta, o que adiciona um custo extra.
- **Tempo total (`totalMs`)**: é a soma simples das três fases. Útil para uma visão consolidada, mas as fases individuais revelam melhor as diferenças entre as estruturas.

### Interpretação dos dados estruturais

**Para a Árvore AVL:**

- `treeHeight` deve estar no intervalo `[⌊log₂(n+1)⌋, ~1.44 × log₂(n)]`. Por exemplo, para 10.000 elementos, espera-se altura entre 13 e 20. Se a altura for significativamente maior que o limite superior teórico, há indício de bug no balanceamento.
- A altura da AVL afeta diretamente `putMs`, `getMs` e `deleteMs`: cada unidade adicional de altura representa um nível extra de comparação em cada operação.

**Para a Tabela Hash:**

- `bucketCount` é o tamanho atual do vetor após todos os redimensionamentos disparados durante as inserções. Deve ser um número primo da sequência `{7, 17, 37, 79, ..., 350899}`.
- `loadFactor` = `elementCount / bucketCount`. O valor máximo teórico é 0,75 (limiar de redimensionamento), mas na prática fica entre 0,37 e 0,75 dependendo de quantos redimensionamentos ocorreram. Um `loadFactor` próximo de 0,75 indica que a tabela está próxima de um novo redimensionamento; um valor baixo (ex.: 0,20) indica que a tabela foi recém-redimensionada para um tamanho muito maior que o necessário.
- `maxChainLength` revela a pior cadeia de colisão. Em uma tabela saudável, este valor deve ser pequeno (1 a 5 para cargas de até 100.000 elementos com boa função de hash). Um `maxChainLength` muito elevado (ex.: 50+) indica que as chaves não estão sendo bem distribuídas pela função de dispersão ou que o tamanho da tabela é inadequado.

### Comparação de alocação de memória

- O `allocatedBytes` da **Hash** inclui o custo de todos os `HashEntry` criados mais as realocações do vetor de baldes a cada `Resize`. Para benchmarks com muitos elementos, os redimensionamentos podem representar uma fração significativa do total alocado.
- O `allocatedBytes` da **AVL** inclui exclusivamente os `AvlNode` criados (um por elemento). Cada `AvlNode` armazena mais campos que um `HashEntry` (altura + duas referências vs. apenas uma referência `Next`), mas não há realocações de um vetor subjacente.
- A comparação do `allocatedBytes` entre as duas estruturas para a mesma carga `N` revela o *overhead* de memória relativo de cada implementação.

### Exemplo de análise

Para `N = 100.000` operações:

| Métrica | AVL (esperado) | Hash (esperado) |
|---------|---------------|-----------------|
| `treeHeight` | 17 a 24 | — |
| `bucketCount` | — | 175.447 |
| `loadFactor` | — | ≈ 0,57 |
| `maxChainLength` | — | 1 a 5 |
| `allocatedBytes` | ~3,2 MB | ~6,4 MB (com redimensionamentos) |

A Hash tende a ser mais rápida em `getMs`, mas aloca significativamente mais bytes devido às realocações do vetor. A AVL tende a ter `putMs` e `deleteMs` maiores devido ao custo das rotações de balanceamento, mas é mais previsível (variância menor entre execuções).

## Limitações

O benchmark possui as seguintes limitações metodológicas que devem ser consideradas ao interpretar os resultados:

1. **Ausência de aquecimento da JIT**: as primeiras chamadas de método incluem o custo de compilação *just-in-time* do .NET, o que pode inflar artificialmente os tempos da primeira execução. Execuções subsequentes tenderiam a ser mais rápidas.

2. **Chaves sequenciais e previsíveis**: as chaves `"benchmark-key-0"` a `"benchmark-key-N-1"` são strings curtas e altamente previsíveis. Na Tabela Hash, isso favorece uma boa distribuição (hash polinomial de strings curtas produz boa dispersão). Na Árvore AVL, a inserção em ordem crescente é o pior caso de desbalanceamento e, portanto, exercita intensamente o mecanismo de rotações. Isso torna o benchmark **mais rigoroso para a AVL** do que um cenário com chaves aleatórias seria.

3. **Medição de memória global à thread**: `GC.GetAllocatedBytesForCurrentThread()` captura todas as alocações da thread, incluindo quaisquer alocações internas do runtime .NET que possam ocorrer durante o benchmark. Embora o impacto seja pequeno, ele introduz um ruído de fundo inevitável.

4. **Coleta de lixo em segundo plano**: durante a fase de remoção, os objetos removidos tornam-se elegíveis para coleta, mas o GC pode ou não executar durante o benchmark. Isso não afeta o `allocatedBytes` (que mede apenas alocações, não desalocações), mas pode afetar os tempos medidos se uma coleta ocorrer durante uma das fases.

5. **Ausência de repetições estatísticas**: cada chamada ao endpoint executa o benchmark uma única vez. Não há cálculo de média, mediana, desvio-padrão ou intervalo de confiança. Os resultados de uma única execução podem ser afetados por variações transientes de carga do sistema operacional. Para uma análise estatística rigorosa, recomenda-se executar o benchmark múltiplas vezes e agregar os resultados.

6. **Operações homogêneas**: todas as operações são do mesmo tipo (Put de N chaves, Get de N chaves, Delete de N chaves). Cenários realistas de uso de uma tabela de símbolos envolvem um misto intercalado de inserções, buscas e remoções, o que pode produzir comportamentos diferentes dos observados neste benchmark de fases segregadas.
