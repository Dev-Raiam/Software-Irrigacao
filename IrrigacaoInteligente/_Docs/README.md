# IrrigacaoInteligente - Controle de Automação e Irrigação

Serviço de borda (.NET Worker) executado em CLP/Raspberry Pi (Linux ARM64) que faz a ponte entre a nuvem e o hardware de campo: recebe comandos via **MQTT**, controla módulos por **Modbus**, coleta **telemetria** e mantém credenciais/estado localmente.

## Arquitetura

Arquitetura baseada em **Vertical Slice** com **CQRS** via **Mediator** (`Toolbox.Core.Api`). O host é um **Worker Service** (`Microsoft.NET.Sdk.Worker`) que também expõe alguns endpoints HTTP mínimos para configuração. A comunicação é bidirecional via **MQTT** (broker remoto/nuvem ↔ broker local/hardware), com leitura de campo via **Modbus** (`NModbus`/serial).

### Estrutura de Pastas

```
IrrigacaoInteligente/
│
├── Program.cs                                  Bootstrap (Serilog, DI, RateLimiter, endpoints)
│
├── Configurations/                             SETUP DE INFRAESTRUTURA
│   ├── AuthenticacaoConfiguracao.cs            (JWT Bearer)
│   ├── ContextoConfiguracao.cs                 (EF Core + SQLite)
│   └── InjecaoDependenciaConfiguracao.cs       (DI: serviços, workers, DataProtection)
│
├── Features/                                   REGRAS DE NEGÓCIO (Vertical Slice + CQRS)
│   ├── Configuracao/
│   │   └── ValidarEstadoAplicacaoHandler.cs    (valida se credenciais estão prontas)
│   ├── Credenciais/                            Gerenciamento de credenciais
│   │   ├── AdicionarCredenciais.cs
│   │   ├── AtualizarCredenciais.cs
│   │   ├── CredenciaisHandler.cs
│   │   ├── GerenciadorCredenciais.cs
│   │   └── _Interfaces/ICriptografia.cs
│   ├── Hardware/
│   │   ├── Controle/                           Comandos de atuação
│   │   │   └── (Acionar/Desligar Bomba, MotoBomba, Solenoid, Inversor;
│   │   │        Abrir/Fechar Válvula; Definir Frequência/Válvula Proporcional)
│   │   └── Sensores/                           Leitura de sensores
│   │       └── (LerSensor Corrente, Distancia, Nivel, Ph, Pressao,
│   │            Temperatura, Tensao, Umidade; LerMonitorPosicao)
│   ├── Sincronizacao/
│   │   ├── SincronizarControladoresHandler.cs  Sync API → Banco Local
│   │   └── _Interfaces/IAutomacaoApi.cs
│   ├── Telemetria/
│   │   ├── PublicarTelemetria.cs
│   │   ├── SalvarTelemetria.cs
│   │   └── Tekon/TekonTelemetria.cs
│   └── _Shared/Abstractions/
│       ├── ChavesBanco.cs
│       └── IMqttCliente.cs
│
├── Domain/                                      MODELO DE DOMÍNIO
│   ├── Entities/
│   │   ├── Configuracao/Configuracao.cs
│   │   ├── Controlador/Controlador.cs
│   │   ├── Hardware/ (ComandoAnalogico, ComandoDigital, Operacao)
│   │   └── Telemetria/ (Telemetria, TelemetriaResposta,
│   │                    BlocoTelemetriaProtocolo, BlocoTelemetriaSinal)
│   └── Enums/ (Dispositivo, Interface, Modulo, Porta)
│
├── Infrastructure/
│   ├── Auth/                                   Token JWT
│   │   ├── GerenciadorToken.cs
│   │   └── IAutenticacaoApi.cs
│   ├── Http/                                   Clientes HTTP
│   │   ├── AutenticacaoApi.cs
│   │   ├── AutomacaoApi.cs
│   │   └── ManipuladorTokenAcesso.cs           (DelegatingHandler: injeta Bearer)
│   ├── Mqtt/                                   Comunicação MQTT
│   │   ├── MqttCliente.cs                      (base)
│   │   ├── MqttClienteRemoto.cs               (broker nuvem)
│   │   └── MqttClienteLocal.cs                (broker local/hardware)
│   ├── Data/                                   EF Core + SQLite
│   │   ├── IrrigacaoInteligenteContext.cs
│   │   ├── Mappers/                            (configurações de entidades)
│   │   └── Migrations/
│   ├── Criptografia/Criptografia.cs
│   └── SeedData/SeedData.cs
│
├── Presentation/Endpoints/
│   └── Credenciais.cs                          POST/PUT /configuracao/credenciais
│
├── State/                                      ESTADO EM MEMÓRIA (Singletons)
│   ├── Aplicacao.cs                            (gates de prontidão: App e MQTT)
│   ├── CredenciaisAplicacao.cs
│   ├── ArmazenamentoToken.cs                   (JWT ativo)
│   ├── ArmazenamentoAutomacao.cs               (dados sincronizados)
│   ├── ApiOptions.cs
│   └── MqttConfiguracao.cs
│
└── Workers/                                    BACKGROUND SERVICES
    ├── ProntidaoWorker.cs                      (aguarda credenciais → libera app)
    ├── MqttWorker.cs                           (gerencia conexões MQTT)
    └── Telemetria/TekonWorker.cs              (leitura Modbus + telemetria Tekon)
```

> `SincronizacaoWorker` e `SincronizarAutomacao` aparecem comentados na DI (`InjecaoDependenciaConfiguracao`) — a sincronização ativa hoje é feita por `SincronizarControladoresHandler`.

## Fluxos Principais

### 1. Inicialização do Sistema (Gate de Prontidão)

```
ProntidaoWorker (loop a cada 5s)
  → Aplicacao.ValidarEstadoAplicacao()
      → mediator.Execute(ValidarEstadoAplicacao)   (verifica credenciais no SQLite)
  → quando válido: Aplicacao.LiberarAplicacao()     ← libera o gate _pronto
```

O estado `Aplicacao` expõe **dois gates** baseados em `TaskCompletionSource`:

- **`_pronto`** — liberado por `LiberarAplicacao()`; workers aguardam em `AguardarLiberacaoAplicacao()`.
- **`_prontoMqtt`** — liberado por `LiberarMqtt()`; o `MqttWorker` aguarda em `AguardarLiberacaoMqtt()`.

### 2. Conexões MQTT

```
MqttWorker
  → aguarda gate de MQTT (AguardarLiberacaoMqtt)
  → conecta no broker LOCAL  (MqttConfiguracao: Servidor/Porta/Usuario/Senha)
      → assina "telemetria/resposta"
  → conecta no broker REMOTO (broker.freemqtt.com:1883)
      → assina "comando/{painelId}"
  → registra callbacks de mensageria e de desconexão
```

### 3. Telemetria via Modbus (Tekon)

```
TekonWorker
  → aguarda liberação da aplicação
  → lê controlador master de ArmazenamentoAutomacao
  → loop (a cada 2s):
      → ReadHoldingRegisters / ReadCoils (NModbus)
      → decodifica modelo → telemetria
      → grava JSON em {Log:Path}/tekon.json
```

### 4. Roteamento de Comandos via MQTT

```
Nuvem/API → publica no broker REMOTO: "comando/{painelId}"
   payload: { "$type": "...Comando, Toolbox.Automacao.Irrigacao", ... }

MqttClienteRemoto (callback)
  → desserialização polimórfica pelo $type
  → mediator.Execute((dynamic)comando)
      → Handler em Features/Hardware/Controle
          → reencaminha ao broker LOCAL → hardware/CLP
```

### 5. Autenticação JWT (Automática)

```
ManipuladorTokenAcesso (DelegatingHandler)
  → usa token de ArmazenamentoToken se válido
  → se expirado: GerenciadorToken → AutenticacaoApi.AutenticarAsync(...)
  → injeta Bearer token nas requisições do AutomacaoApi
```

## Padrões Utilizados

### Vertical Slice Architecture

Cada feature contém Command + Handler — sem camadas horizontais compartilhadas.

### CQRS via Mediator

- Handlers retornam `ResponseResult`.
- `mediator.Execute((dynamic)command)` — late binding para resolução em runtime pelo tipo concreto.
- Registro automático via `AddMediator(Assembly.GetExecutingAssembly(), typeof(AcionarBomba).Assembly)`.

### Gate de Prontidão (TaskCompletionSource)

`Aplicacao` usa dois `TaskCompletionSource` para bloquear workers dependentes até a aplicação/MQTT estarem prontos.

### Data Protection

Chaves persistidas em sistema de arquivos via `PersistKeysToFileSystem`, com caminho configurável por `DataProtection:KeysPath` (fallback: `{BaseDirectory}/Keys`). Em produção (Raspberry/Linux), recomenda-se um diretório persistente fora do deploy (ex.: `/var/lib/irrigacao/keys`).

### Rate Limiting

Política `limite-tentativas` (Concurrency Limiter: `PermitLimit=5`, `QueueLimit=5`) aplicada aos endpoints de credenciais.

## Tópicos MQTT

| Tópico                | Broker | Direção           | Descrição                          |
| --------------------- | ------ | ----------------- | ---------------------------------- |
| `comando/{painelId}`  | Remoto | Nuvem → Worker    | Comandos vindos da nuvem           |
| `telemetria/resposta` | Local  | Hardware → Worker | Resposta de telemetria do hardware |

## Dependências Externas

| Pacote                                                | Uso                                      |
| ----------------------------------------------------- | ---------------------------------------- |
| `Toolbox.Core.Api` (NuGet)                            | Mediator, Handlers, ResponseResult       |
| `Toolbox.Automacao.Irrigacao` (ProjectRef)            | Tipos de Comando, Drivers, Modelos Tekon |
| `NModbus` / `NModbus.Serial` / `System.IO.Ports`      | Comunicação Modbus serial com hardware   |
| `MQTTnet`                                             | Cliente MQTT                             |
| `Newtonsoft.Json`                                     | Serialização polimórfica (`$type`)       |
| `Microsoft.EntityFrameworkCore.Sqlite`                | Persistência local                       |
| `Microsoft.AspNetCore.Authentication.JwtBearer`       | Autenticação JWT                         |
| `Microsoft.AspNetCore.DataProtection`                 | Proteção de dados/credenciais            |
| `Serilog.AspNetCore` / `Sinks.Console` / `Sinks.File` | Logging estruturado                      |

## Como Executar

### Pré-requisitos

- .NET 10.0
- Broker MQTT local e remoto acessíveis
- SQLite (criado automaticamente via migrations + `SeedData`)

### Configuração (appsettings.json / appsettings.Development.json)

```json
{
  "Log": { "Path": "Logs" },
  "DataProtection": { "KeysPath": "/var/lib/irrigacao/keys" },
  "MqttConfiguracao": {
    "Servidor": "localhost",
    "Porta": 1883,
    "Usuario": "usuario",
    "Senha": "senha"
  },
  "ApiOptions": {
    "BaseUrl": "https://api.toolbox.app.br"
  }
}
```

> As credenciais sensíveis são configuradas via endpoint `POST /configuracao/credenciais` e armazenadas criptografadas no SQLite local. Em desenvolvimento, também é possível usar **User Secrets** (`UserSecretsId` definido no `.csproj`).

### Executar (desenvolvimento)

```bash
dotnet run
```

### Publicar (produção — Raspberry Pi / Linux ARM64)

O perfil de `Release` já está configurado para gerar um binário único, self-contained:

```bash
dotnet publish -c Release
```

Gera executável `linux-arm64`, `PublishSingleFile`, `SelfContained`, sem símbolos de debug.

## Endpoints HTTP

| Método | Rota                        | Descrição            | Proteções                  |
| ------ | --------------------------- | -------------------- | -------------------------- |
| `POST` | `/configuracao/credenciais` | Adiciona credenciais | Auth + `limite-tentativas` |
| `PUT`  | `/configuracao/credenciais` | Atualiza credenciais | Auth + `limite-tentativas` |

## Workers

| Worker            | Frequência                  | Responsabilidade                                                      |
| ----------------- | --------------------------- | --------------------------------------------------------------------- |
| `ProntidaoWorker` | loop a cada 5s até pronto   | Valida credenciais e libera o gate da aplicação (`Aplicação pronta.`) |
| `MqttWorker`      | loop a cada 5s até conectar | Conecta brokers local e remoto e assina tópicos                       |
| `TekonWorker`     | loop a cada 2s              | Lê Modbus do controlador master e grava telemetria em `tekon.json`    |

## Logs

Configurados via Serilog em `Program.cs`, com caminho base em `Log:Path`:

- **Desenvolvimento:** Console + arquivo (`{Log:Path}/log-AAAAMMDD.txt`).
- **Produção:** somente arquivo.
- **Rotação:** diária (`RollingInterval.Day`), mantendo os últimos 7 arquivos.

## Adicionar Novo Comando

1. Criar classe de comando em `Toolbox.Automacao.Irrigacao/Comandos/`.
2. Criar handler em `Features/Hardware/Controle/` (ou categoria correspondente).
3. Handler processa o comando e publica no broker local quando aplicável.
4. O mediator registra automaticamente via `Assembly.GetExecutingAssembly()`.
