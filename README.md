# EncurtadorUfabc

Encurtador de URLs desenvolvido como projeto da disciplina de Algoritmos e Estruturas de Dados II (AED2). O objetivo é comparar o desempenho de duas estruturas de dados diferentes — **Tabela Hash** e **Árvore AVL** — usadas como tabela de símbolos (`ISymbolTable<TKey, TValue>`) para armazenar e resolver as URLs encurtadas.

## Estrutura do projeto

A solução (`EncurtadorUfabc.slnx`) é organizada em múltiplos projetos .NET, separados por responsabilidade:

```
EncurtadorUfabc.slnx          # Arquivo de solução, referencia todos os projetos
src/
  EncurtadorUfabc.API/        # Projeto Web (ASP.NET Core Minimal APIs) - ponto de entrada da aplicação
  EncurtadorUfabc.Core/       # Contratos, modelos, persistência (EF Core) e código transversal
  EncurtadorUfabc.AVL/        # Implementação da tabela de símbolos usando Árvore AVL
  EncurtadorUfabc.Hash/       # Implementação da tabela de símbolos usando Tabela Hash
tests/
  EncurtadorUfabc.Tests/      # Testes automatizados
docs/                         # Documentação complementar do projeto
```

### `EncurtadorUfabc.API`

Projeto executável (`Microsoft.NET.Sdk.Web`) que expõe os endpoints HTTP. Usa o padrão **Vertical Slice Architecture**: cada operação da API vive em sua própria pasta dentro de `Endpoints/`, contendo tudo o que aquele endpoint precisa (rota, handler, filtros).

Os endpoints estão duplicados em dois grupos, um para cada estrutura de dados avaliada:

```
Endpoints/
  Avl/                # Endpoints que usam a tabela de símbolos AVL (rota base /avl)
    CreateUrl/
    DeleteUrl/
    ListUrls/
    ResolveUrl/
    Benchmark/
  Hash/               # Endpoints que usam a tabela de símbolos Hash (rota base /hash)
    CreateUrl/
    DeleteUrl/
    ListUrls/
    ResolveUrl/
    Benchmark/
```

Cada endpoint implementa a interface `IEndpoint` (definida em `EncurtadorUfabc.Core/Crosscutting`) e é registrado automaticamente por reflection em `Program.cs`, sem necessidade de registro manual de rotas.

Rotas disponíveis (mesmo conjunto para `/avl` e `/hash`):

| Método | Rota                       | Descrição                                                                       |
| ------ | -------------------------- | -------------------------------------------------------------------------------- |
| POST   | `/{avl\|hash}/urls`        | Cria uma URL curta                                                                |
| GET    | `/{avl\|hash}/urls`        | Lista as URLs cadastradas                                                         |
| GET    | `/{avl\|hash}/urls/{code}` | Resolve o código e redireciona para a URL original (`?raw=true` retorna JSON)     |
| DELETE | `/{avl\|hash}/urls/{code}` | Remove uma URL curta                                                              |
| GET    | `/{avl\|hash}/benchmark`   | Executa um benchmark das operações da estrutura                                  |

### `EncurtadorUfabc.Core`

Biblioteca compartilhada com:

- `Contracts/`: DTOs de request/response e a interface `ISymbolTable<TKey, TValue>` usada por AVL e Hash.
- `Models/`: modelo de domínio `ShortUrl`.
- `Persistence/`: `AppDbContext` (EF Core + SQLite) e entidade `ShortUrlEntity`.
- `Crosscutting/`: `IEndpoint`, extensões de registro de endpoints e filtros de log.

### `EncurtadorUfabc.AVL` e `EncurtadorUfabc.Hash`

Cada projeto contém uma implementação independente de `ISymbolTable<TKey, TValue>`:

- **AVL** (`AvlNode`, `AvlSymbolTable`): árvore binária de busca balanceada.
- **Hash** (`HashSymbolTable`): tabela hash.

Ambas são registradas no `Program.cs` como serviços *keyed* (`"avl"` e `"hash"`), permitindo que os endpoints escolham qual estrutura usar via `[FromKeyedServices]`.

### `tests/EncurtadorUfabc.Tests`

Projeto de testes automatizados da solução.

## Como rodar a aplicação localmente

### Pré-requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) instalado.

### Passos

1. Clone o repositório e entre na pasta do projeto:

   ```bash
   git clone <url-do-repositorio>
   cd dsa-II-project
   ```

2. Restaure as dependências (opcional, o `dotnet run` faz isso automaticamente):

   ```bash
   dotnet restore
   ```

3. Execute a API a partir da pasta do projeto `EncurtadorUfabc.API`:

   ```bash
   cd src/EncurtadorUfabc.API
   dotnet run
   ```

   Ou, a partir da raiz do repositório:

   ```bash
   dotnet run --project src/EncurtadorUfabc.API/EncurtadorUfabc.API.csproj
   ```

4. A API sobe nos endereços definidos em `Properties/launchSettings.json`:
   - HTTP: `http://localhost:5143`
   - HTTPS: `https://localhost:7249`

5. Com a aplicação em execução, acesse o Swagger para explorar e testar os endpoints:

   ```
   http://localhost:5143/swagger
   ```

O banco de dados SQLite (`encurtador.db`) é criado automaticamente na primeira execução, via `db.Database.EnsureCreated()` no `Program.cs`.
