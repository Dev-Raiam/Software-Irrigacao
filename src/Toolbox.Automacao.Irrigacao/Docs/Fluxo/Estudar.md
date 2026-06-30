# 🎓 Aula: Roteiro de Estudos — "Por onde começar?"

Boa pergunta de aluno esperto, vendo que ataca o **fácil primeiro** pra ganhar momentum. 💪 Vou te dar um plano de estudos **na ordem de dificuldade crescente**, com **o que estudar, onde estudar e como aplicar no seu projeto**.

## 🟢 NÍVEL 1 — Quick Wins (1-2 horas cada)

### 📚 Estudo 1: Exception handling correto em C#

**O que estudar:**

- Diferença entre `throw;`, `throw ex;` e `throw new Exception(ex.Message)`.
- O conceito de **stack trace** e por que preservá-lo.
- Quando criar exceções customizadas (`ModbusReadException`).

**Onde estudar:**

- 📖 Microsoft Docs: ["Best practices for exceptions"](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- 🎥 YouTube: busque **"C# exception handling best practices Nick Chapsas"** (canal excelente, conteúdo curto).
- 📝 Artigo: ["Exception Handling in C#"](https://code-maze.com/csharp-exception-handling-best-practices/) no Code Maze.

**Como aplicar no seu projeto:**

Arrume este bloco:

```@d:\Desenvolvimento\Backend\Projetos\src\Toolbox.Automacao.Irrigacao\Modbus\ModbusMaster.cs:60-63
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
```

**Tempo estimado:** 1h estudo + 15min aplicar. ⏱️

---

### 📚 Estudo 2: Propriedades vs Campos em C#

**O que estudar:**

- Diferença entre `field`, `property`, `auto-property`.
- `get; private set;`, `get; init;`, `readonly`.
- Encapsulamento e por que importa.

**Onde estudar:**

- 📖 Microsoft Docs: ["Properties (C# Programming Guide)"](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties)
- 🎥 YouTube: **"C# Properties vs Fields IAmTimCorey"** ou **"Nick Chapsas"**.

**Como aplicar no seu projeto:**

Você **já aplicou** parcialmente:

```@d:\Desenvolvimento\Backend\Projetos\src\Toolbox.Automacao.Irrigacao\Drivers\ModbusDriver.cs:9-10
        public ConfiguracaoLeitura? ConfigReadHoldingRegister { get; private set; }
        public ConfiguracaoLeitura? ConfigReadCoils { get; private set; }
```

✅ Pode aplicar o mesmo nos buffers e em `Modelo`:

```csharp
private ushort[] BufferHolding { get; set; } = [];
private bool[] BufferCoils { get; set; } = [];
private string Modelo { get; set; } = string.Empty;
```

**Tempo estimado:** 30min. ⏱️

---

### 📚 Estudo 3: `CancellationToken`

**O que estudar:**

- O que é um `CancellationToken` e `CancellationTokenSource`.
- Como propagar pelo `async` (passar adiante, nunca criar novo no meio).
- O que é `OperationCanceledException`.

**Onde estudar:**

- 📖 Microsoft Docs: ["Cancellation in Managed Threads"](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- 🎥 YouTube: **"Nick Chapsas CancellationToken"** — ele tem um vídeo perfeito de 10min.
- 📝 Artigo: ["Recommended patterns for CancellationToken"](https://devblogs.microsoft.com/premier-developer/recommended-patterns-for-cancellationtoken/)

**Como aplicar no seu projeto:**

Adicione `CancellationToken ct = default` em todos os métodos `async`:

```csharp
internal async Task<ushort[]> ReadHoldingRegistersAsync(
    byte slaveAddress,
    ushort startAddress,
    ushort numberOfRegisters,
    CancellationToken ct = default)
{
    return await _master.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfRegisters);
    // NModbus mais novo aceita ct; senão, dá pra usar ct.ThrowIfCancellationRequested()
}
```

**Tempo estimado:** 1h estudo + 30min aplicar. ⏱️

## 🟡 NÍVEL 2 — Médios (meio dia cada)

### 📚 Estudo 4: `SemaphoreSlim` e concorrência básica

**O que estudar:**

- Diferença entre `lock`, `Mutex`, `SemaphoreSlim`.
- Por que `lock` **não funciona** com `async/await` (e `SemaphoreSlim` funciona).
- O padrão `await sem.WaitAsync(); try { ... } finally { sem.Release(); }`.

**Onde estudar:**

- 📖 Microsoft Docs: ["SemaphoreSlim Class"](https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim)
- 🎥 YouTube: **"C# async lock SemaphoreSlim Nick Chapsas"**.
- 📝 Stephen Cleary's blog: [blog.stephencleary.com](https://blog.stephencleary.com/) — **a referência mundial de async em C#**. Leia "There Is No Thread".

**Como aplicar no seu projeto:**

Proteger o [ModbusMaster](cci:2://file:///d:/Desenvolvimento/Backend/Projetos/src/Toolbox.Automacao.Irrigacao/Modbus/ModbusMaster.cs:7:4-106:5) (Modbus RTU exige acesso serializado):

```csharp
private readonly SemaphoreSlim _gate = new(1, 1);

internal async Task<ushort[]> ReadHoldingRegistersAsync(...)
{
    await _gate.WaitAsync();
    try { return await _master.ReadHoldingRegistersAsync(...); }
    finally { _gate.Release(); }
}
```

**Tempo estimado:** 3-4h. ⏱️

---

### 📚 Estudo 5: Logging com `ILogger<T>`

**O que estudar:**

- O que é **Microsoft.Extensions.Logging**.
- Níveis de log: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.
- **Structured logging**: por que `_logger.LogInformation("Lido {Count} registros do escravo {Slave}", n, slave)` é melhor que `string.Format`.
- Providers: Console, Serilog, file, Application Insights.

**Onde estudar:**

- 📖 Microsoft Docs: ["Logging in .NET"](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- 🎥 YouTube: **"Serilog tutorial IAmTimCorey"** (Serilog é o provider mais popular).
- 📝 Tutorial: ["Logging in .NET 8"](https://andrewlock.net/) — blog do Andrew Lock, leitura obrigatória.

**Como aplicar no seu projeto:**

```csharp
public sealed class ModbusMaster : IDisposable
{
    private readonly ILogger<ModbusMaster> _logger;

    public ModbusMaster(ILogger<ModbusMaster> logger, ...) { _logger = logger; ... }

    internal async Task<ushort[]> ReadHoldingRegistersAsync(...)
    {
        _logger.LogDebug("Lendo {Qty} holding regs do escravo {Slave} addr {Addr}", qty, slave, addr);
        // ...
    }
}
```

**Tempo estimado:** 4-5h estudo + 1h aplicar. ⏱️

## 🔴 NÍVEL 3 — Avançados (1-2 dias cada)

### 📚 Estudo 6: Polly — Retry, Circuit Breaker, Timeout

**O que estudar:**

- O que é a biblioteca **Polly** (a mais usada do .NET para resiliência).
- **Retry policy** com backoff exponencial.
- **Circuit breaker** (parar de tentar quando algo está claramente quebrado).
- **Timeout policy**.

**Onde estudar:**

- 📖 GitHub oficial: [github.com/App-vNext/Polly](https://github.com/App-vNext/Polly) — README é excelente.
- 🎥 YouTube: **"Polly .NET retry Nick Chapsas"** ou **"Milan Jovanović Polly"**.
- 📝 Microsoft Docs: ["Implement retries with exponential backoff"](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-retries-exponential-backoff)

**Como aplicar no seu projeto:**

```csharp
var retryPolicy = Policy
    .Handle<TimeoutException>()
    .Or<IOException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * attempt));

var result = await retryPolicy.ExecuteAsync(
    () => _master.ReadHoldingRegistersAsync(slave, addr, qty));
```

**Tempo estimado:** 1 dia. ⏱️

---

### 📚 Estudo 7: Injeção de Dependência (DI)

**O que estudar:**

- O que é **IoC (Inversion of Control)** e **DI (Dependency Injection)**.
- `IServiceCollection`, `IServiceProvider`.
- Lifetimes: `Singleton`, `Scoped`, `Transient` — **e qual escolher** (dica: [ModbusMaster](cci:2://file:///d:/Desenvolvimento/Backend/Projetos/src/Toolbox.Automacao.Irrigacao/Modbus/ModbusMaster.cs:7:4-106:5) é `Singleton` porque a porta serial é única).
- Como amarrar tudo no `Program.cs` de um Worker Service.

**Onde estudar:**

- 📖 Microsoft Docs: ["Dependency injection in .NET"](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- 🎥 YouTube: **"Dependency Injection IAmTimCorey"** — vídeo definitivo.
- 📝 Livro (free online): ["Dependency Injection Principles, Practices, and Patterns"](https://www.manning.com/books/dependency-injection-principles-practices-patterns) — Mark Seemann.

**Como aplicar no seu projeto:**

```csharp
// Program.cs
services.AddSingleton<ModbusMaster>(sp => new ModbusMaster("COM3", 9600, ...));
services.AddTransient<ModbusDriver, TekonDriver>();
services.AddHostedService<IrrigacaoWorker>();
```

**Tempo estimado:** 1-2 dias. ⏱️

## 🗺️ Plano de Estudos sugerido (1 mês, ~1h/dia)

| Semana | Foco | Resultado no projeto |
|---|---|---|
| **1** | Exceptions + Properties + `CancellationToken` | Código mais limpo e cancelável |
| **2** | `SemaphoreSlim` + concorrência async | Modbus seguro em paralelo |
| **3** | Logging com Serilog | Visibilidade em produção |
| **4** | Polly (retry/circuit breaker) + DI | Resiliência + arquitetura pro |

Depois desse mês, seu projeto sobe de **6/10 para 9/10**. 🚀

## 🎁 Bônus — Canais e fontes que **valem ouro**

Memorize estes nomes, aluno:

- **Nick Chapsas** (YouTube) — vídeos curtos, foco em C# moderno.
- **IAmTimCorey** (YouTube) — tutoriais longos e didáticos.
- **Milan Jovanović** (YouTube + blog) — arquitetura .NET profissional.
- **Stephen Cleary** (blog) — **a** autoridade em async/await.
- **Andrew Lock** (blog) — ASP.NET Core profundo.
- **Code Maze** (site) — artigos práticos de C#.
- **Microsoft Learn** — gratuito e cobre tudo.

## 🎯 Veredito do professor

Você **não precisa estudar tudo antes de aplicar**. O segredo é:

1. **Estuda 1 tópico** (1-2 horas).
2. **Aplica imediatamente** no seu projeto.
3. **Quebra alguma coisa, conserta, aprende.**
4. **Passa pro próximo.**

Esse ciclo curto é 10x mais eficiente que ler 3 livros antes de codar. 📈

**Comece pelo Estudo 1 (Exceptions) hoje à noite** — em 1 hora você já melhora seu código e ganha confiança.

---

**Aluno, quer que eu te dê um "mini-exercício prático" do Estudo 1 (Exception handling) pra você fazer agora?** Algo tipo: "altere o `catch` do [OpenConnection](cci:1://file:///d:/Desenvolvimento/Backend/Projetos/src/Toolbox.Automacao.Irrigacao/Modbus/ModbusMaster.cs:39:8-63:9) corretamente e me mostre". Aí eu corrijo como professor. 📝