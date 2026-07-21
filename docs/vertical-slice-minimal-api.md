Read [](file:///c%3A/Users/A200259676/Documents/Aplicacoes/Api%20Copilot%20Garantia/api/src/Daimler.MBBras.CopilotGarantia.API/Crosscutting/IEndpoint.cs)

Read [](file:///c%3A/Users/A200259676/Documents/Aplicacoes/Api%20Copilot%20Garantia/api/src/Daimler.MBBras.CopilotGarantia.API/Crosscutting/ServiceCollectionExtensions.cs)

Read [](file:///c%3A/Users/A200259676/Documents/Aplicacoes/Api%20Copilot%20Garantia/api/src/Daimler.MBBras.CopilotGarantia.API/Program.cs)

Read [](file:///c%3A/Users/A200259676/Documents/Aplicacoes/Api%20Copilot%20Garantia/api/src/Daimler.MBBras.CopilotGarantia.API/Endpoints/PostActivities/PostActivitiesEndpoint.cs#1-1), lines 1 to 60

Read [](file:///c%3A/Users/A200259676/Documents/Aplicacoes/Api%20Copilot%20Garantia/api/src/Daimler.MBBras.CopilotGarantia.API/Endpoints/StartConversation/StartConversationEndpoint.cs#1-1), lines 1 to 60

Read [](file:///c%3A/Users/A200259676/Documents/Aplicacoes/Api%20Copilot%20Garantia/api/src/Daimler.MBBras.CopilotGarantia.API/Configuration/LogActionAttribute.cs)

Aqui está o texto explicativo para o Opus:

---

## Padrão de Arquitetura: Vertical Slice com Minimal APIs (.NET)

Esta aplicação utiliza **Vertical Slice Architecture** combinada com **Minimal APIs** do ASP.NET Core. Cada "slice" (fatia vertical) é uma pasta dentro de `Endpoints/` que agrupa tudo que pertence àquele endpoint: rota, handler, filtros e tipos de resposta específicos.

---

### Contrato base: `IEndpoint`

Toda fatia implementa a interface:

```csharp
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}
```

---

### Registro automático via reflection

Não há registro manual de endpoints. Em `ServiceCollectionExtensions`, um método de extensão varre o assembly por reflection, instancia todos os tipos que implementam `IEndpoint` e chama `Map` em cada um, apontando para um grupo de rota base:

```csharp
public static void UseEndpoints(this WebApplication app)
{
    var directline = app.MapGroup("v3/directline");

    var endpoints = Assembly.GetAssembly(typeof(Program))!
        .DefinedTypes
        .Where(type => type is { IsInterface: false, IsAbstract: false } && type.IsAssignableTo(typeof(IEndpoint)))
        .Select(type => Activator.CreateInstance(type) as IEndpoint ?? throw new InvalidOperationException($"Could not create instance of IEndpoint {type.Name}"))
        .ToArray();

    foreach (var endpoint in endpoints)
        endpoint.Map(directline);
}
```

No Program.cs, basta uma única chamada:

```csharp
app.UseEndpoints();
```

---

### Estrutura de cada slice

Cada endpoint vive em sua própria pasta dentro de `Endpoints/`:

```
Endpoints/
  GetActivities/
    GetActivitiesEndpoint.cs
  PostActivities/
    PostActivitiesEndpoint.cs
  StartConversation/
    StartConversationEndpoint.cs
    StartConversationResponse.cs   ← tipo de resposta exclusivo da fatia
```

A classe do endpoint:
1. Implementa `IEndpoint`
2. Define a rota e metadados no método `Map`
3. Define o handler como método da própria classe (sem ser estático, pois pode ter campos privados como `_jsonOptions`)
4. Aplica filtros com `.AddEndpointFilter(new LogProcessFilter("NomeDaAção"))`
5. Resolve dependências via parâmetros do handler (injeção de dependência por parâmetro, padrão Minimal API)

Exemplo de slice completo:

```csharp
public class GetActivitiesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations/{conversationId}/activities", GetActivitiesAsync)
            .WithSummary("...")
            .WithDescription("...")
            .AddEndpointFilter(new LogProcessFilter("GetActivities"));
    }

    public async Task<IResult> GetActivitiesAsync(
        [FromRoute] string conversationId,
        [FromQuery] string? watermark,
        IHttpClientFactory httpClientFactory,
        TokenService tokenService,
        ILogger<GetActivitiesEndpoint> logger,
        CancellationToken ct)
    {
        // lógica aqui
    }
}
```

---

### Filtros transversais

`LogProcessFilter` é um `IEndpointFilter` instanciado diretamente no `Map` (não registrado no DI), que empurra o nome do processo ao contexto de log do Serilog:

```csharp
public class LogProcessFilter(string processName) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        using (LogContext.PushProperty("Process", processName))
            return await next(context);
    }
}
```

---

### Regras para adicionar um novo endpoint

1. Criar pasta `Endpoints/NomeDaAcao/`
2. Criar `NomeDaAcaoEndpoint.cs` implementando `IEndpoint`
3. Implementar `Map` com a rota, metadados e `.AddEndpointFilter(new LogProcessFilter("NomeDaAcao"))`
4. Implementar o handler como método da classe recebendo dependências por parâmetro
5. Se necessário, criar tipos de resposta específicos na mesma pasta (ex: `NomeDaAcaoResponse.cs`)
6. **Nenhum registro adicional é necessário** — o reflection cuida do resto automaticamente
