# Capítulo 5 — Estrutura do Projeto

Este capítulo é um passeio guiado pelas pastas do projeto. Use-o como mapa ao navegar o código.

## 5.1 Mapa de pastas

```
IrrigacaoInteligente/
│
├── Program.cs                 Bootstrap (Serilog, DI, RateLimiter, endpoints)
│
├── Configurations/            Setup de infraestrutura
│   ├── AuthenticacaoConfiguracao.cs      (JWT Bearer)
│   ├── ContextoConfiguracao.cs           (EF Core + SQLite)
│   └── InjecaoDependenciaConfiguracao.cs (DI: serviços, workers, DataProtection)
│
├── Features/                  Regras de negócio (Vertical Slice + CQRS)
│   ├── Configuracao/          Valida se as credenciais estão prontas
│   ├── Credenciais/           Gerenciamento de credenciais
│   ├── Hardware/
│   │   ├── Controle/          Comandos de atuação
│   │   └── Sensores/          Leitura de sensores
│   ├── Sincronizacao/         Sync API → banco local
│   ├── Telemetria/            Publicação/persistência de telemetria
│   └── _Shared/               Abstrações compartilhadas
│
├── Domain/                    Modelo de domínio
│   ├── Entities/              Configuracao, Controlador, Hardware, Telemetria
│   └── Enums/                 Dispositivo, Interface, Modulo, Porta
│
├── Infrastructure/
│   ├── Auth/                  Token JWT
│   ├── Http/                  Clientes HTTP (auth, automação, handler de token)
│   ├── Mqtt/                  Clientes MQTT (base, remoto, local)
│   ├── Data/                  EF Core + SQLite (contexto, mappers, migrations)
│   ├── Criptografia/          Criptografia de credenciais
│   └── SeedData/              Carga inicial
│
├── State/                     Estado em memória (Singletons)
│   ├── Aplicacao.cs           Gates de prontidão (App e MQTT)
│   ├── CredenciaisAplicacao.cs
│   ├── ArmazenamentoToken.cs  JWT ativo
│   ├── ArmazenamentoAutomacao.cs  Dados sincronizados
│   ├── ApiOptions.cs
│   └── MqttConfiguracao.cs
│
└── Workers/                   Background services
    ├── ProntidaoWorker.cs     Aguarda credenciais → libera app
    ├── MqttWorker.cs          Gerencia conexões MQTT
    └── Telemetria/TekonWorker.cs   Leitura Modbus + telemetria
```

## 5.2 Responsabilidade de cada pasta

- **Configurations/** — onde a infraestrutura é "ligada" ao container de DI. Tudo que `Program.cs` chama para montar a aplicação está aqui.
- **Features/** — o coração das regras de negócio. Cada subpasta é um grupo de funcionalidades; cada arquivo de handler é uma fatia vertical.
- **Domain/** — entidades e enums puros, sem dependência de infraestrutura.
- **Infrastructure/** — implementações concretas de tudo que fala com o mundo externo (rede, disco, hardware).
- **State/** — objetos *singleton* que guardam estado em memória durante a execução.
- **Workers/** — serviços de fundo que mantêm o sistema vivo (loops de prontidão, MQTT e telemetria).

## 5.3 Observações importantes do código atual

- O worker de prontidão ativo registrado na DI é o **`ProntidaoWorkerCopy`**, não o `ProntidaoWorker` original:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:65-69
        services.AddHostedService<ProntidaoWorkerCopy>();
        //services.AddHostedService<ProntidaoWorker>();
        // services.AddHostedService<SincronizacaoWorker>();
        services.AddHostedService<MqttWorker>();
        //services.AddHostedService<TekonWorker>();
```

- Vários serviços estão **comentados** (`SincronizacaoWorker`, `TekonWorker`), indicando que estão desativados na configuração atual. A pasta `Features/XXXXXCredenciaisObsoleta/` também sinaliza código legado em transição.
- Alguns handlers e entidades estão **comentados por inteiro** (ex.: `AcionarBombaHandler.cs`, `Telemetria.cs`), refletindo uma refatoração em andamento. Considere isso ao usar o código como referência.

---

Anterior: [Capítulo 4 — Visão Arquitetural](04-Visao-Arquitetural.md) · Próximo: [Capítulo 6 — Modelo de Domínio](06-Modelo-de-Dominio.md)
