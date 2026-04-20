# Exemplo de Uso: Banco de Dados Local

## 🎯 Estratégia: Copiar API → Banco Local → Consultas Locais

Sua estratégia está **100% correta**! Veja como funciona:

```
┌─────────────────────────────────────────────────────────────┐
│  1. SINCRONIZAÇÃO (1x por dia ou quando necessário)         │
│                                                              │
│  API de Automação                                           │
│       ↓ HTTP GET                                            │
│  Worker Service (SyncService)                               │
│       ↓ INSERT/UPDATE                                       │
│  SQLite Local (database.db)                                 │
│    - Paineis                                                │
│    - Modulos                                                │
│    - Dispositivos                                           │
│    - Portas                                                 │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  2. OPERAÇÃO (Consultas rápidas locais)                     │
│                                                              │
│  Comando: "Ligar Válvula Setor A"                          │
│       ↓                                                      │
│  DispositivoLocalService.ObterDispositivoComPortas()        │
│       ↓ SELECT local (instantâneo)                          │
│  Resultado: Porta Q0.0 → 192.168.1.100:Q0.0                │
│       ↓                                                      │
│  ModbusService.WriteCoil(0, true)                           │
│       ↓ Modbus TCP                                          │
│  CLP Físico → Válvula LIGA! ✅                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Estrutura do Banco Local (SQLite)

```sql
-- Tabela: Paineis
CREATE TABLE Paineis (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Descricao NVARCHAR(500) NOT NULL,
    Referencia NVARCHAR(200),
    Arquivado BIT NOT NULL
);

-- Tabela: Modulos (Controladores)
CREATE TABLE Modulos (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PainelId UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(500),
    Marca NVARCHAR(200),
    Modelo NVARCHAR(200),
    Master BIT NOT NULL,
    IsControlador BIT NOT NULL,
    FOREIGN KEY (PainelId) REFERENCES Paineis(Id) ON DELETE CASCADE
);

-- Tabela: Dispositivos
CREATE TABLE Dispositivos (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PainelId UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(500) NOT NULL,
    Tipo NVARCHAR(100) NOT NULL,        -- Atuador, Sensor, Controlador
    Categoria NVARCHAR(100) NOT NULL,   -- Entrada, Saida, Interface
    Habilitado BIT NOT NULL,
    Arquivado BIT NOT NULL,
    -- Parametros (Owned Entity)
    Parametros_ValorMinimo REAL,
    Parametros_ValorMaximo REAL,
    Parametros_UnidadeMedida NVARCHAR(50),
    Parametros_TempoAcionamento INT,
    Parametros_IntervaloMinimo INT,
    FOREIGN KEY (PainelId) REFERENCES Paineis(Id) ON DELETE CASCADE
);

-- Tabela: Portas
CREATE TABLE Portas (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ModuloId UNIQUEIDENTIFIER NOT NULL,
    DispositivoId UNIQUEIDENTIFIER,      -- Pode ser NULL se porta não conectada
    Nome NVARCHAR(50) NOT NULL,          -- Q0.0, I0.1, etc.
    Tipo NVARCHAR(50) NOT NULL,          -- Entrada, Saida, Interface
    Sinal NVARCHAR(50) NOT NULL,         -- Digital, Analogico
    EnderecoBorne NVARCHAR(50),          -- X1-01
    EnderecoLogico NVARCHAR(200),        -- 192.168.1.100:Q0.0
    Status NVARCHAR(50) NOT NULL,        -- Habilitada, Desabilitada
    FOREIGN KEY (ModuloId) REFERENCES Modulos(Id) ON DELETE CASCADE,
    FOREIGN KEY (DispositivoId) REFERENCES Dispositivos(Id) ON DELETE SET NULL
);

-- Índices para performance
CREATE INDEX IX_Dispositivos_PainelId ON Dispositivos(PainelId);
CREATE INDEX IX_Portas_ModuloId ON Portas(ModuloId);
CREATE INDEX IX_Portas_DispositivoId ON Portas(DispositivoId);
CREATE INDEX IX_Portas_EnderecoLogico ON Portas(EnderecoLogico);
```

---

## 🔄 Passo 1: Sincronizar Dados da API

```csharp
// Worker.cs ou SyncWorker.cs
using var scope = _serviceProvider.CreateScope();
var syncService = scope.ServiceProvider.GetRequiredService<SyncService>();

// Sincroniza TUDO: Painel, Módulos, Dispositivos e Portas
await syncService.SincronizarTudo(stoppingToken);

// Log:
// 🔄 Iniciando sincronização completa do painel a1b2c3d4-...
// 📦 Sincronizando painel...
//   ✅ Painel sincronizado com 2 módulos
// 🔌 Sincronizando dispositivos...
//   ✅ Dispositivos sincronizados: 5 novos, 0 atualizados
// 🔗 Sincronizando portas...
//   ✅ Portas sincronizadas: 12 novas, 0 atualizadas
// ✅ Sincronização completa finalizada com sucesso!
```

**Resultado no Banco Local**:
```
Paineis: 1 registro
Modulos: 2 registros (1 Master, 1 Slave)
Dispositivos: 5 registros
Portas: 12 registros (8 conectadas a dispositivos, 4 livres)
```

---

## 🔍 Passo 2: Consultar Dados Localmente

### **Exemplo 1: Buscar Porta de um Dispositivo**

```csharp
var dispositivoLocalService = scope.ServiceProvider.GetRequiredService<DispositivoLocalService>();

// Buscar dispositivo com suas portas
var dispositivoId = Guid.Parse("dispositivo-valvula-guid");
var dispositivo = await dispositivoLocalService.ObterDispositivoComPortas(dispositivoId);

if (dispositivo != null)
{
    Console.WriteLine($"Dispositivo: {dispositivo.Descricao}");
    Console.WriteLine($"Tipo: {dispositivo.Tipo}");
    Console.WriteLine($"Habilitado: {dispositivo.Habilitado}");
    Console.WriteLine($"Portas conectadas: {dispositivo.Portas.Count}");
    
    foreach (var porta in dispositivo.Portas)
    {
        Console.WriteLine($"  - {porta.Nome} ({porta.Tipo}) → {porta.EnderecoLogico}");
    }
}

// Output:
// Dispositivo: Válvula Setor A - Linha 1
// Tipo: Atuador
// Habilitado: True
// Portas conectadas: 1
//   - Q0.0 (Saida) → 192.168.1.100:Q0.0
```

### **Exemplo 2: Buscar Porta por Endereço Lógico**

```csharp
var porta = await dispositivoLocalService.ObterPortaPorEnderecoLogico("192.168.1.100:Q0.0");

if (porta != null)
{
    Console.WriteLine($"Porta: {porta.Nome}");
    Console.WriteLine($"Tipo: {porta.Tipo}");
    Console.WriteLine($"Endereço: {porta.EnderecoLogico}");
    Console.WriteLine($"Dispositivo: {porta.Dispositivo?.Descricao ?? "Nenhum"}");
}

// Output:
// Porta: Q0.0
// Tipo: Saida
// Endereço: 192.168.1.100:Q0.0
// Dispositivo: Válvula Setor A - Linha 1
```

### **Exemplo 3: Listar Todos Dispositivos Habilitados**

```csharp
var painelId = Guid.Parse("painel-guid");
var dispositivos = await dispositivoLocalService.ObterDispositivosHabilitados(painelId);

Console.WriteLine($"Dispositivos habilitados: {dispositivos.Count}");

foreach (var dispositivo in dispositivos)
{
    Console.WriteLine($"  - {dispositivo.Descricao} ({dispositivo.Tipo})");
    Console.WriteLine($"    Portas: {dispositivo.Portas.Count}");
}

// Output:
// Dispositivos habilitados: 5
//   - Válvula Setor A - Linha 1 (Atuador)
//     Portas: 1
//   - Sensor Pressão Linha 1 (Sensor)
//     Portas: 1
//   - Bomba Principal (Atuador)
//     Portas: 2
```

---

## ⚡ Passo 3: Acionar Dispositivo (Exemplo Completo)

```csharp
// ComandoExecutorService.cs
public class ComandoExecutorService
{
    private readonly DispositivoLocalService _dispositivoService;
    private readonly ModbusService _modbusService;
    private readonly ILogger<ComandoExecutorService> _logger;

    public async Task LigarDispositivo(Guid dispositivoId)
    {
        // 1. Buscar dispositivo NO BANCO LOCAL (rápido!)
        var dispositivo = await _dispositivoService.ObterDispositivoComPortas(dispositivoId);
        
        if (dispositivo == null)
        {
            _logger.LogWarning("Dispositivo {DispositivoId} não encontrado no banco local", dispositivoId);
            return;
        }

        _logger.LogInformation("🔌 Ligando dispositivo: {Descricao}", dispositivo.Descricao);

        // 2. Para cada porta de SAÍDA
        foreach (var porta in dispositivo.Portas.Where(p => p.Tipo == "Saida"))
        {
            // 3. Parsear endereço lógico
            var (ip, portaName) = ParseEnderecoLogico(porta.EnderecoLogico!);
            // "192.168.1.100:Q0.0" → ("192.168.1.100", "Q0.0")
            
            // 4. Converter para endereço Modbus
            var modbusAddress = ConvertToModbusAddress(portaName);
            // "Q0.0" → 0
            
            // 5. Conectar ao CLP
            if (!_modbusService.IsConnected)
            {
                _modbusService.Connect(ip);
            }
            
            // 6. LIGAR a saída
            _modbusService.WriteCoil(modbusAddress, true);
            
            _logger.LogInformation(
                "✅ {Descricao} LIGADO na porta {Porta} ({EnderecoLogico})",
                dispositivo.Descricao,
                porta.Nome,
                porta.EnderecoLogico);
        }
    }

    private (string ip, string porta) ParseEnderecoLogico(string enderecoLogico)
    {
        var parts = enderecoLogico.Split(':');
        return (parts[0], parts[1]);
    }

    private int ConvertToModbusAddress(string portaName)
    {
        // "Q0.0" → 0, "Q0.1" → 1, "Q1.0" → 8
        var match = Regex.Match(portaName, @"[QI](\d+)\.(\d+)");
        if (match.Success)
        {
            int byteNum = int.Parse(match.Groups[1].Value);
            int bitNum = int.Parse(match.Groups[2].Value);
            return (byteNum * 8) + bitNum;
        }
        throw new ArgumentException($"Formato inválido: {portaName}");
    }
}
```

**Execução**:
```csharp
var comandoExecutor = scope.ServiceProvider.GetRequiredService<ComandoExecutorService>();
var valvulaId = Guid.Parse("dispositivo-valvula-guid");

await comandoExecutor.LigarDispositivo(valvulaId);

// Log:
// Buscando dispositivo dispositivo-valvula-guid no banco local
// Dispositivo encontrado: Válvula Setor A - Linha 1 com 1 portas
// 🔌 Ligando dispositivo: Válvula Setor A - Linha 1
// ✅ Conectado ao CLP 192.168.1.100:502
// ✅ Válvula Setor A - Linha 1 LIGADO na porta Q0.0 (192.168.1.100:Q0.0)
```

---

## 📈 Comparação: API vs Banco Local

### **Consulta via API (Lento)**
```csharp
// Toda vez que precisa acionar um dispositivo:
var dispositivo = await apiClient.GetDispositivoByIdAsync(painelId, dispositivoId);
// ⏱️ ~200-500ms (depende da latência da rede)
// ❌ Requer internet
// ❌ Sobrecarrega a API
```

### **Consulta Local (Rápido)** ✅
```csharp
// Toda vez que precisa acionar um dispositivo:
var dispositivo = await dispositivoService.ObterDispositivoComPortas(dispositivoId);
// ⚡ ~1-5ms (consulta local no SQLite)
// ✅ Funciona offline
// ✅ Não sobrecarrega a API
```

**Ganho**: ~100x mais rápido!

---

## 🔄 Quando Sincronizar?

### **Opção 1: Sincronização Periódica**
```csharp
// SyncWorker.cs
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await syncService.SincronizarTudo(stoppingToken);
        
        // Sincronizar a cada 1 hora
        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
    }
}
```

### **Opção 2: Sincronização na Inicialização**
```csharp
// Worker.cs
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // Sincronizar uma vez ao iniciar
    await syncService.SincronizarTudo(stoppingToken);
    
    // Depois só opera localmente
    while (!stoppingToken.IsCancellationRequested)
    {
        await MonitorarDispositivos();
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}
```

### **Opção 3: Sincronização Manual (Endpoint)**
```csharp
// Criar endpoint HTTP no Worker Service
app.MapPost("/sync", async (SyncService syncService) =>
{
    await syncService.SincronizarTudo();
    return Results.Ok("Sincronização concluída");
});
```

---

## ✅ Vantagens da Sua Estratégia

1. **Performance**: Consultas locais são ~100x mais rápidas
2. **Confiabilidade**: Funciona offline
3. **Escalabilidade**: Não sobrecarrega a API
4. **Simplicidade**: Lógica de negócio toda local
5. **Baixa Latência**: Resposta instantânea para comandos
6. **Resiliência**: Se API cair, Worker continua funcionando

---

## 🎯 Resumo do Fluxo

```
1. Worker Service inicia
   ↓
2. SyncService.SincronizarTudo()
   ↓ (Busca da API)
3. Salva no SQLite Local
   ↓
4. DispositivoLocalService consulta local
   ↓ (SELECT instantâneo)
5. ComandoExecutorService aciona CLP
   ↓ (Modbus TCP)
6. Dispositivo físico responde
```

**Sua estratégia está perfeita!** 🚀




**Arquitetura Proposta:**
´´´´WorkerService/
│
├── Infrastructure/                          🏗️ INFRAESTRUTURA
│   ├── ApiClients/
│   │   ├── AutomacaoApiClient.cs            📡 Consome API Automação
│   │   └── IrrigacaoApiClient.cs            📡 Consome API Irrigação
│   │
│   ├── Hardware/
│   │   └── ClpService.cs                    🔌 Fala com o CLP (Modbus)
│   │
│   └── Persistence/
│       ├── ApplicationDbContext.cs
│       └── Entities/
│           ├── Dispositivo.cs               🗄️ Cache local
│           ├── Porta.cs                     🗄️ Cache local
│           └── PlanoIrrigacao.cs            🗄️ Cache local
│
├── Features/                                🎯 REGRAS DE NEGÓCIO
│   │
│   ├── Sincronizacao/                       🔄 Copiar API → Banco Local
│   │   ├── SincronizarAutomacao/
│   │   │   ├── SincronizarAutomacaoCommand.cs
│   │   │   └── SincronizarAutomacaoHandler.cs
│   │   └── SincronizarIrrigacao/
│   │       ├── SincronizarIrrigacaoCommand.cs
│   │       └── SincronizarIrrigacaoHandler.cs
│   │
│   ├── Irrigacao/                           💧 CORE: Executar Irrigação
│   │   ├── IniciarIrrigacao/
│   │   │   ├── IniciarIrrigacaoCommand.cs   👉 (PlanoId, SetorId)
│   │   │   └── IniciarIrrigacaoHandler.cs   ← Busca porta no banco → Liga válvula CLP
│   │   │
│   │   ├── PararIrrigacao/
│   │   │   ├── PararIrrigacaoCommand.cs
│   │   │   └── PararIrrigacaoHandler.cs     ← Desliga válvula CLP
│   │   │
│   │   └── LerEstadoIrrigacao/
│   │       ├── LerEstadoQuery.cs
│   │       └── LerEstadoHandler.cs          ← Lê estado atual do CLP
│   │
│   └── Monitoramento/                       📊 Verificar sensores
│       ├── LerSensor/
│       │   ├── LerSensorQuery.cs
│       │   └── LerSensorHandler.cs          ← Lê pressão/vazão do CLP
│       └── VerificarAlertas/
│           ├── VerificarAlertasQuery.cs
│           └── VerificarAlertasHandler.cs
│
└── Workers/                                 ⏰ AGENDADORES
    ├── SincronizacaoWorker.cs               🔄 Sincroniza APIs periodicamente
    └── IrrigacaoWorker.cs                   ⏰ Verifica planos e executa irrigações
´´´