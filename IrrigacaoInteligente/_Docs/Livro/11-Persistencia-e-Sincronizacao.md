# Capítulo 11 — Persistência e Sincronização

## 11.1 Persistência local com EF Core + SQLite

O sistema mantém estado em um banco **SQLite** local, acessado via **Entity Framework Core**. A infraestrutura de dados fica em `Infrastructure/Data/`:

```
Infrastructure/Data/
├── IrrigacaoInteligenteContext.cs   DbContext principal
├── Mappers/                         Configurações de mapeamento das entidades
└── Migrations/                      Migrations do EF Core
```

O registro do contexto é feito em `Configurations/ContextoConfiguracao.cs` (chamado por `Program.cs` via `RegistrarContexto`). O arquivo do banco é o `IrrigacaoInteligente.db` na raiz do projeto, criado automaticamente por migrations + `SeedData`.

### Por que SQLite

- **Sem servidor:** ideal para borda (um único arquivo, zero administração).
- **Resiliência offline:** o dispositivo guarda credenciais e configuração mesmo sem nuvem.
- **Leve:** baixo consumo de recursos em Raspberry Pi.

## 11.2 Seed de dados

No bootstrap, `Program.cs` chama o seed antes de iniciar a aplicação:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:80
await SeedData.Seed(app.Services);
```

`Infrastructure/SeedData/SeedData.cs` garante que o banco esteja criado e populado com os dados iniciais necessários.

## 11.3 Estado em memória vs. persistência

É importante distinguir dois tipos de estado:

| Tipo | Onde | Exemplos | Vida útil |
| ---- | ---- | -------- | --------- |
| **Persistente** | SQLite (EF Core) | credenciais, configuração de controladores | sobrevive a reinícios |
| **Em memória** | Singletons de `State/` | `ArmazenamentoToken`, `ArmazenamentoAutomacao`, gates de prontidão | dura enquanto o processo roda |

Os singletons de `State/` são registrados na DI:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:42-45
        services.AddSingleton<ArmazenamentoToken>();
        services.AddSingleton<CredenciaisAplicacao>();
        services.AddSingleton<ArmazenamentoAutomacao>();
        services.AddSingleton<ICriptografia, Criptografia>();
```

- **`ArmazenamentoAutomacao`** guarda os dados sincronizados (controladores, dispositivos, portas) usados pelos handlers de hardware para resolver comandos rapidamente.
- **`ArmazenamentoToken`** guarda o JWT ativo (ver Capítulo 12).

## 11.4 Sincronização: nuvem → banco local

A sincronização traz a configuração dos controladores da API de nuvem para o estado local. Fica em `Features/Sincronizacao/`:

- **`SincronizarControladoresHandler`** — comando ativo que faz o sync API → banco/estado.
- **`_Interfaces/IAutomacaoApi`** — contrato do cliente HTTP da API de automação (implementado em `Infrastructure/Http/AutomacaoApi.cs`).

```
API de Automação (nuvem)
   │  GET controladores/dispositivos/portas
   ▼
SincronizarControladoresHandler
   │  grava
   ▼
Banco SQLite + ArmazenamentoAutomacao (memória)
```

> **Nota:** Existe um `SincronizarAutomacao` e um `SincronizacaoWorker` **comentados** na DI. A sincronização ativa hoje é disparada pelo `SincronizarControladoresHandler`, e não por um worker dedicado.

## 11.5 Cliente HTTP da automação

`Infrastructure/Http/AutomacaoApi.cs` é registrado como `HttpClient` tipado e recebe automaticamente o token JWT via `ManipuladorTokenAcesso`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:71-74
        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services
            .AddHttpClient<IAutomacaoApi, AutomacaoApi>()
            .AddHttpMessageHandler<ManipuladorTokenAcesso>();
```

---

Anterior: [Capítulo 10 — Hardware: Comandos e Sensores](10-Hardware-Comandos-e-Sensores.md) · Próximo: [Capítulo 12 — Segurança](12-Seguranca.md)
