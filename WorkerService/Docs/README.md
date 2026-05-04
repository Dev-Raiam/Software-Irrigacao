# WorkerService - Controle de Automação e Irrigação

## Arquitetura

Arquitetura baseada em **Vertical Slice** com **CQRS** e **Mediator Pattern**, comunicação bidirecional via **MQTT** (broker remoto ↔ broker local) e sincronização periódica com API REST.

### Estrutura de Pastas

```
WorkerService/
│
├── Features/                                   REGRAS DE NEGÓCIO
│   ├── Configuracao/
│   │   ├── ConfiguracaoSistema/                Credenciais e Inicialização
│   │   │   ├── AdicionarCredenciais.cs
│   │   │   ├── AtualizarCredenciais.cs
│   │   │   ├── ConfigurarSistemaHandler.cs
│   │   │   └── ConfiguracaoInicializacaoHandler.cs
│   │   └── Credenciais/                        Gerenciamento de Credenciais
│   │       ├── GerenciadorCredenciais.cs
│   │       └── _Interfaces/ICriptografia.cs
│   │
│   ├── Sincronizacao/Automacao/                Sync API → Banco Local
│   │   ├── SincronizarAutomacao.cs             (orquestrador)
│   │   ├── SincronizarPaineisHandler.cs
│   │   ├── SincronizarModulosHandler.cs
│   │   ├── SincronizarPortasHandler.cs
│   │   ├── SincronizarInterfacesHandler.cs
│   │   └── SincronizarDispositivosHandler.cs
│   │
│   └── Roteadores/                             Roteamento de Comandos MQTT
│       ├── Controle/                           (AcionarBomba, AbrirValvula, etc.)
│       ├── Sensores/                           (LerSensor*)
│       └── Sincronizacao/                      (SincronizarPaineis, etc.)
│
├── Infrastructure/
│   ├── Auth/                                   Token JWT
│   │   ├── GerenciadorToken.cs
│   │   ├── ManipuladorTokenAcesso.cs
│   │   └── IAutenticacaoApi.cs
│   ├── Http/                                   Clientes HTTP
│   │   ├── AutenticacaoApi.cs
│   │   └── AutomacaoApi.cs
│   ├── Mqtt/                                   Comunicação MQTT
│   │   ├── MqttCliente.cs                      (base)
│   │   ├── MqttClienteRemoto.cs                (broker nuvem)
│   │   └── MqttClienteLocal.cs                 (broker local/hardware)
│   ├── Data/                                   EF Core + SQLite
│   │   └── WorkerServiceContext.cs
│   └── Criptografia/
│       └── Criptografia.cs
│
├── State/                                      ESTADO EM MEMÓRIA (Singletons)
│   ├── CredenciaisAplicacao.cs                 (PainelId, ContaId, Integração)
│   ├── ArmazenamentoToken.cs                   (JWT ativo)
│   ├── ArmazenamentoAutomacao.cs               (dados sincronizados)
│   ├── ConfiguracaoInicializacao.cs            (gate de prontidão)
│   ├── ApiConfiguracao.cs
│   └── MqttConfiguracao.cs
│
└── Workers/                                    BACKGROUND SERVICES
    ├── ProntidaoWorker.cs                      (aguarda credenciais)
    ├── SincronizacaoWorker.cs                  (sync a cada 30s)
    └── MqttWorker.cs                           (gerencia conexões MQTT)
```

## Fluxos Principais

### 1. Inicialização do Sistema

```
ProntidaoWorker (loop a cada 10s)
  → ConfiguracaoInicializacao.Iniciar()
      → IniciarConfiguracaoInicializacaoHandler
          → GerenciadorCredenciais (lê credenciais do SQLite)
          → CredenciaisAplicacao (carrega em memória)
  → ConfiguracaoInicializacaoConcluida()  ← libera os outros Workers
```

Os outros workers bloqueiam em `AguardarConfiguracaoInicializacaoAsync()` até este gate ser liberado.

### 2. Sincronização de Automação

```
SincronizacaoWorker (loop a cada 30s)
  → aguarda gate de prontidão
  → SincronizarAutomacao.Executar()
      → SincronizarPaineisHandler     → API REST → SQLite + ArmazenamentoAutomacao
      → SincronizarModulosHandler     → API REST → SQLite
      → SincronizarPortasHandler      → API REST → SQLite
      → SincronizarInterfacesHandler  → API REST → SQLite
      → SincronizarDispositivosHandler → API REST → SQLite
```

### 3. Roteamento de Comandos via MQTT

```
Sistema Externo / API
  → publica no broker REMOTO: "comando/{painelId}"
      payload: { "$type": "...AcionarBomba, Toolbox.Automacao.Irrigacao", "id": "...", ... }

MqttClienteRemoto (callback)
  → JsonConvert.DeserializeObject (TypeNameHandling.Objects)
      → instancia tipo concreto pelo $type
  → mediator.Execute((dynamic)command)
      → AcionarBombaHandler
          → serializa comando com $type
          → publica no broker LOCAL: "comando/{painelId}"

Hardware / CLP local
  → MqttClienteLocal recebe e executa
```

### 4. Autenticação JWT (Automática)

```
ManipuladorTokenAcesso (DelegatingHandler)
  → verifica se token em ArmazenamentoToken é válido
  → se expirado: GerenciadorToken.ObterTokenAsync()
      → AutenticacaoApi.AutenticarAsync(chave, segredo, contextoId)
      → armazena novo token
  → injeta Bearer token em todas as requisições HTTP
```

## Padrões Utilizados

### Vertical Slice Architecture
Cada feature contém Command + Handler — sem camadas horizontais compartilhadas.

### CQRS via Mediator
- `ICommandHandler<T>` — altera estado, retorna `ResponseResult`
- `mediator.Execute((dynamic)command)` — late binding para resolução em runtime pelo tipo concreto

### Gate de Prontidão (TaskCompletionSource)
`ConfiguracaoInicializacao` usa `TaskCompletionSource` para bloquear workers dependentes até as credenciais serem carregadas.

### Serialização MQTT com TypeNameHandling
Comandos trafegam com `$type` no JSON (Newtonsoft.Json `TypeNameHandling.Objects`), permitindo deserialização polimórfica automática no receptor C#.

## Tópicos MQTT

| Tópico | Direção | Descrição |
|---|---|---|
| `comando/{painelId}` | Remoto → Worker | Comandos vindos da nuvem |
| `comando/{painelId}` | Worker → Local | Reencaminhamento ao hardware |

## Dependências Externas

| Pacote | Uso |
|---|---|
| `Toolbox.Core.Api` (NuGet) | Mediator, CommandHandler, ResponseResult |
| `Toolbox.Automacao.Irrigacao` (ProjectRef) | Tipos de Command (AcionarBomba, etc.) |
| `MQTTnet` | Cliente MQTT |
| `Newtonsoft.Json` | Serialização polimórfica |
| `EF Core + SQLite` | Persistência local |
| `Serilog` | Logging estruturado |

## Como Executar

### Pré-requisitos
- .NET 10.0
- Broker MQTT local e remoto acessíveis
- SQLite (criado automaticamente via migrations)

### Configuração (appsettings.json)
```json
{
  "MqttConfiguracao": {
    "BrokerLocal": "localhost",
    "BrokerRemoto": "broker.exemplo.com",
    "Porta": 1883,
    "UsuarioRemoto": "usuario",
    "SenhaRemoto": "senha"
  },
  "ApiConfiguration": {
    "BaseUrl": "https://api.toolbox.app.br"
  }
}
```

> As credenciais (PainelId, ContaId, Integração) são configuradas via comando `AdicionarCredenciais` e armazenadas criptografadas no SQLite local.

### Executar
```bash
dotnet run
```

## Workers e Logs

| Worker | Frequência | Log esperado |
|---|---|---|
| `ProntidaoWorker` | a cada 10s até pronto | `Aplicação Configurada!!!` |
| `SincronizacaoWorker` | a cada 30s | `Paineis sincronizados`, etc. |
| `MqttWorker` | contínuo | `Conectado ao broker MQTT` |

## Adicionar Novo Comando

1. Criar classe em `Toolbox.Automacao.Irrigacao/Comandos/`
2. Criar handler em `Features/Roteadores/{Categoria}/`
3. Handler implementa `ICommandHandler<NovoComando>` e publica no broker local
4. O mediator registra automaticamente via `Assembly.GetExecutingAssembly()`
