# Capítulo 4 — Visão Arquitetural

## 4.1 Estilo geral

O IrrigacaoInteligente é um **Worker Service** (`Microsoft.NET.Sdk.Worker`) que também expõe alguns endpoints HTTP mínimos para configuração. A organização do código segue **Vertical Slice Architecture** com **CQRS** mediado por um **Mediator** (pacote `Toolbox.Core.Api`).

```
[ HTTP Endpoints ]   [ Workers (BackgroundService) ]   [ MQTT Callbacks ]
        \                      |                              /
         \                     v                             /
          \-------->   Mediator (CQRS)   <-----------------/
                              |
                   Handlers (Vertical Slices)
                              |
        +---------------------+----------------------+
        |              |              |              |
     Domain      Infrastructure     State        Toolbox libs
```

## 4.2 Por que Vertical Slice + CQRS

Em vez de camadas horizontais (Controllers → Services → Repositories), cada **funcionalidade** vive em sua própria "fatia": o comando e seu handler ficam juntos. Isso traz:

- **Coesão:** tudo de uma feature em um só lugar.
- **Baixo acoplamento:** adicionar um comando novo não mexe em código existente.
- **Descoberta automática:** o mediator registra os handlers por reflexão de assembly.

O registro do mediator acontece em `RegistrarServicos`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:28-31
        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );
```

Note que ele registra **dois assemblies**: o próprio projeto (handlers locais) e o assembly de `Toolbox.Automacao.Irrigacao` (onde vivem os tipos de comando).

## 4.3 Despacho dinâmico de comandos

Comandos chegam por MQTT como JSON polimórfico (campo `$type`). Após desserializar, o tipo concreto só é conhecido em runtime, então o sistema usa *late binding*:

```csharp
mediator.Execute((dynamic)comando);
```

O `(dynamic)` faz o CLR resolver, em tempo de execução, o handler correto para o tipo concreto do comando. Handlers retornam um `ResponseResult` padronizado.

## 4.4 Componentes de runtime

| Componente | Papel |
| ---------- | ----- |
| **Program.cs** | Bootstrap: Serilog, configuração, DI, RateLimiter, endpoints, seed |
| **Workers** | Serviços de fundo de longa duração (prontidão, MQTT, telemetria) |
| **State (Singletons)** | Estado em memória compartilhado (prontidão, token, automação) |
| **Infrastructure** | MQTT, HTTP, EF Core/SQLite, criptografia, auth |
| **Domain** | Entidades e enums do negócio |
| **Features** | Handlers CQRS por funcionalidade |

## 4.5 Bidirecionalidade

A comunicação é **bidirecional** e atravessa dois brokers MQTT:

- **Broker remoto (nuvem):** recebe comandos (`comando/{painelId}`).
- **Broker local (hardware):** envia comandos ao CLP e recebe telemetria (`telemetria/resposta`).

Esse desenho permite que a borda funcione como tradutor entre o mundo da nuvem e o mundo industrial.

## 4.6 Resiliência e prontidão

Workers não começam a operar imediatamente. Eles aguardam **gates de prontidão** (baseados em `TaskCompletionSource`) que só liberam quando as pré-condições estão satisfeitas (credenciais válidas, MQTT liberado). Isso evita erros de inicialização em ordem indefinida (ver Capítulo 7).

---

Anterior: [Capítulo 3 — Conceitos do Domínio](03-Conceitos-do-Dominio.md) · Próximo: [Capítulo 5 — Estrutura do Projeto](05-Estrutura-do-Projeto.md)
