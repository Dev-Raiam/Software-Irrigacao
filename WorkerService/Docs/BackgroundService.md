# BackgroundService — Workers

## 1. Como parar um Worker

### Parar apenas o Worker (interno)
Usar `break` dentro do `ExecuteAsync` é suficiente. Quando o método retorna, o worker encerra — nenhuma thread fica bloqueada e o consumo de memória é negligível (apenas a instância singleton do DI permanece viva).

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // lógica...
        if (condicaoDeEncerramento)
            break; // Worker encerra aqui
    }
}
```

### Parar toda a aplicação
Injete `IHostApplicationLifetime` e chame `StopApplication()`. Isso cancela o `stoppingToken` de **todos** os workers.

```csharp
public class MeuWorker(IHostApplicationLifetime _lifetime) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // falha irrecuperável:
        _lifetime.StopApplication();
        return Task.CompletedTask;
    }
}
```

### Parar externamente via `CancellationTokenSource`
Útil quando outra classe precisa sinalizar o encerramento do worker.

```csharp
public class MeuWorker : BackgroundService
{
    private readonly CancellationTokenSource _cts = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cts.Token);
        while (!linked.Token.IsCancellationRequested)
        {
            // lógica...
        }
    }

    public void Parar() => _cts.Cancel();
}
```

> **Atenção:** Uma vez encerrado, o `BackgroundService` **não reinicia automaticamente**. O `ExecuteAsync` é chamado uma única vez por ciclo de vida da aplicação. Para rodar novamente, é necessário reiniciar a aplicação.

---

## 2. Verificar conexão com a Internet

Não é necessária nenhuma biblioteca externa — tudo está na BCL do .NET.

### `NetworkInterface.GetIsNetworkAvailable()` — rápido, mas superficial
```csharp
using System.Net.NetworkInformation;

bool temRede = NetworkInterface.GetIsNetworkAvailable();
```
> ⚠️ Só verifica se há adaptador de rede ativo. Não garante acesso real à internet.

### `Ping` para IP conhecido — recomendado para Workers
```csharp
using System.Net.NetworkInformation;

using var ping = new Ping();
var reply = await ping.SendPingAsync("8.8.8.8", timeout: 3000);
bool temInternet = reply.Status == IPStatus.Success;
```
> ✅ Equilíbrio ideal entre velocidade e confiabilidade. Usa o DNS do Google (`8.8.8.8`).

### `HttpClient` HEAD request — mais confiável
```csharp
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
var response = await http.SendAsync(
    new HttpRequestMessage(HttpMethod.Head, "https://www.google.com"),
    stoppingToken
);
bool temInternet = response.IsSuccessStatusCode;
```
> ✅ Verifica conectividade de ponta a ponta. Ideal quando o serviço depende de uma API específica.
