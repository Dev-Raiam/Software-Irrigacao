# WorkerService - Controle de Hardware e Irrigação

## 🏗️ Arquitetura

Este projeto implementa uma arquitetura baseada em **Features (Vertical Slice)** otimizada para controle de hardware via Worker Service.

### Estrutura de Pastas

```
WorkerService/
│
├── Infrastructure/                          🏗️ INFRAESTRUTURA
│   ├── ApiClients/
│   │   ├── IAutomacaoApiClient.cs          📡 Interface API Automação
│   │   └── AutomacaoApiClient.cs           📡 Implementação HTTP Client
│   │
│   ├── Hardware/
│   │   ├── IClpService.cs                  🔌 Interface CLP
│   │   └── ClpService.cs                   🔌 Comunicação Modbus/GPIO
│   │
│   └── Persistence/
│       ├── ApplicationDbContext.cs         💾 EF Core DbContext
│       └── Entities/
│           ├── Painel.cs                   🗄️ Entidade Painel
│           ├── Modulo.cs                   🗄️ Entidade Módulo/Controlador
│           ├── Dispositivo.cs              🗄️ Entidade Dispositivo
│           ├── Porta.cs                    🗄️ Entidade Porta (I/O)
│           ├── PlanoIrrigacao.cs           🗄️ Entidade Plano de Irrigação
│           └── SetorIrrigacao.cs           🗄️ Entidade Setor de Irrigação
│
├── Features/                                🎯 REGRAS DE NEGÓCIO (Vertical Slice)
│   │
│   ├── Sincronizacao/                      🔄 Sincronizar API → Banco Local
│   │   └── SincronizarAutomacao/
│   │       ├── SincronizarAutomacaoCommand.cs
│   │       └── SincronizarAutomacaoHandler.cs
│   │
│   ├── Irrigacao/                          💧 CORE: Executar Irrigação
│   │   ├── IniciarIrrigacao/
│   │   │   ├── IniciarIrrigacaoCommand.cs  👉 Comando: Iniciar
│   │   │   └── IniciarIrrigacaoHandler.cs  ← Busca porta → Liga CLP
│   │   │
│   │   ├── PararIrrigacao/
│   │   │   ├── PararIrrigacaoCommand.cs    👉 Comando: Parar
│   │   │   └── PararIrrigacaoHandler.cs    ← Desliga CLP
│   │   │
│   │   └── LerEstadoIrrigacao/
│   │       ├── LerEstadoQuery.cs           👉 Query: Estado
│   │       └── LerEstadoHandler.cs         ← Lê estado do CLP
│   │
│   └── Monitoramento/                      📊 Monitorar Sensores
│       └── LerSensor/
│           ├── LerSensorQuery.cs           👉 Query: Ler Sensor
│           └── LerSensorHandler.cs         ← Lê valor analógico CLP
│
└── Workers/                                 ⏰ BACKGROUND SERVICES
    ├── SincronizacaoWorker.cs              🔄 Sincroniza APIs (30s)
    └── IrrigacaoWorker.cs                  ⏰ Executa planos de irrigação (10s)
```

## 🔄 Fluxos Principais

### 1. Sincronização (API → Banco Local)
```
API Automação → SincronizacaoWorker → SincronizarAutomacaoHandler
                                              ↓
                                    [Painel, Dispositivos, Portas]
                                              ↓
                                        SQLite Local
```

**Frequência:** A cada 30 segundos

### 2. Controle de Irrigação (Comando → Hardware)
```
IrrigacaoWorker → Verifica Planos Ativos
                        ↓
            IniciarIrrigacaoHandler → Busca Porta no Banco
                        ↓
                  ClpService.LigarPorta(enderecoLogico)
                        ↓
                  Hardware (CLP/GPIO)
```

**Frequência:** A cada 10 segundos (verifica setores em execução)

### 3. Monitoramento de Estado (Hardware → Leitura)
```
LerSensorHandler → ClpService.LerValorAnalogicoAsync()
                        ↓
                  Hardware (CLP)
                        ↓
                  Retorna valor (pressão, vazão, etc)
```

## 🎯 Padrões Utilizados

### CQRS (Command Query Responsibility Segregation)
- **Commands**: Alteram estado (IniciarIrrigacao, PararIrrigacao, SincronizarAutomacao)
- **Queries**: Apenas leitura (LerEstado, LerSensor)

### Vertical Slice Architecture
Cada feature é **independente** e contém:
- Command/Query (DTO de entrada)
- Handler (lógica de negócio)
- Todas as dependências necessárias

### Repository Pattern (Implícito via EF Core)
- `ApplicationDbContext` atua como Unit of Work
- `DbSet<T>` atua como Repository

## 🔌 Comunicação com Hardware

### Interface IClpService
```csharp
Task<bool> LigarPortaAsync(string enderecoLogico);
Task<bool> DesligarPortaAsync(string enderecoLogico);
Task<bool> LerEstadoPortaAsync(string enderecoLogico);
Task<double> LerValorAnalogicoAsync(string enderecoLogico);
```

**Implementação Atual:** Mock (ClpService.cs)
**Próximos Passos:** Implementar Modbus TCP/RTU ou GPIO

## 💧 Sistema de Irrigação

### Entidades Principais

**PlanoIrrigacao**
- Nome, Descrição
- HorarioInicio, DuracaoMinutos
- Ativo (bool)
- Setores (collection)

**SetorIrrigacao**
- Nome, Ordem
- DuracaoMinutos
- PortaId (FK para Porta física)
- Status (Aguardando, Executando, Concluído, Cancelado, Erro)

### Fluxo de Execução

1. **IrrigacaoWorker** verifica planos ativos a cada 10s
2. Se horário atual está dentro da janela do plano:
   - Verifica se há setor em execução
   - Se tempo do setor expirou → Para irrigação → Inicia próximo setor
   - Se não há setor executando → Inicia primeiro setor
3. Quando todos os setores concluem → Marca plano como executado → Reseta setores

## 🚀 Como Executar

### Pré-requisitos
- .NET 10.0
- SQLite

### Configuração (appsettings.json)
```json
{
  "ConnectionStrings": {
    "LocalDatabase": "Data Source=automacao.db"
  },
  "ApiSettings": {
    "BaseUrl": "https://api.automacao.com",
    "BearerToken": "seu-token-aqui",
    "PainelId": "guid-do-painel",
    "TimeoutSeconds": 30
  }
}
```

### Executar
```bash
dotnet run
```

## 📊 Logs

Os Workers geram logs estruturados:
- **SincronizacaoWorker**: Sincronizações, erros de API
- **IrrigacaoWorker**: Início/fim de irrigação, transições de setores
- **Handlers**: Operações de hardware, erros de comunicação

## 🔧 Extensibilidade

### Adicionar Nova Feature
1. Criar pasta em `Features/NomeFeature/`
2. Criar Command/Query
3. Criar Handler com lógica de negócio
4. Registrar Handler no `Program.cs`
5. Usar no Worker ou expor via API

### Adicionar Novo Protocolo de Hardware
1. Criar implementação de `IClpService`
2. Registrar no `Program.cs`
3. Configurar parâmetros de conexão

## 📝 Próximos Passos

- [ ] Implementar Modbus TCP/RTU real
- [ ] Adicionar retry policies (Polly)
- [ ] Implementar fila de comandos (Channels)
- [ ] Adicionar testes unitários
- [ ] Criar API REST para controle manual
- [ ] Implementar histórico de execuções
- [ ] Dashboard de monitoramento em tempo real
