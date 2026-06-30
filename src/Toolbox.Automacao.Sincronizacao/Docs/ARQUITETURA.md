# Arquitetura do Módulo Sincronização

## Visão Geral

O módulo `Toolbox.Automacao.Sincronizacao` é responsável por **sincronizar dados de controladores de irrigação** de uma API externa para um banco de dados local, permitindo leitura subsequente através de um provider de dados.

## Funcionalidade Principal

O módulo realiza as seguintes operações:

1. **Sincronização periódica** - Busca controladores de um painel específico na API externa
2. **Persistência local** - Armazena os dados sincronizados em banco de dados (SQLite)
3. **Fornecimento de dados** - Expõe os dados sincronizados para outros módulos através de um provider
4. **Execução em background** - Executa a sincronização automaticamente em intervalos configuráveis

## Estrutura de Diretórios

```
Toolbox.Automacao.Sincronizacao/
├── Core/
│   ├── Abstractions/       # Interfaces do domínio
│   └── Entities/           # Entidades de domínio (Controlador, Módulo, Dispositivo, etc.)
├── Sync/                   # Lógica de sincronização
├── Provider/               # Provider de dados (leitura do banco)
├── Infrastructure/         # Infraestrutura (DbContext, HttpClient)
└── Extensions/             # Configuração e DI
```

## Camadas da Arquitetura

### 1. Core/Abstractions - Contratos do Domínio

**Interfaces principais:**

- **`IApiAutomacao`** - Contrato para comunicação com API externa
  - `ObterControladoresPorPainelAsync(Guid painelId)` - Busca controladores da API

- **`ISincronizarControladores`** - Contrato principal de sincronização
  - `ExecutarAsync(Guid painelId, CancellationToken)` - Executa o processo de sincronização

- **`IProviderSincronizacao`** - Contrato para fornecimento de dados
  - `ObterControlador(CancellationToken)` - Obtém controlador master do banco
  - `ObterModulos(CancellationToken)` - Obtém módulos do controlador master

### 2. Core/Entities - Modelos de Domínio

Entidades que representam a estrutura de controladores de irrigação:

- **`Controlador`** - Controlador principal (contém Módulos, Interfaces, Dispositivos)
- **`Modulo`** - Módulos do controlador (ex: relés, sensores)
- **`Dispositivo`** - Dispositivos conectados (atuadores, sensores)
- **`Interface`** - Interfaces de conexão (portas, bornes)
- **`Porta`** - Portas de entrada/saída
- **`Canal`** - Canais de comunicação
- **`Parametros`** - Parâmetros dinâmicos (usando `JsonExtensionData`)

### 3. Sync - Lógica de Sincronização

**Componentes:**

- **`SincronizarControladores`** - Implementação principal da sincronização
  - Busca controladores na API via `IApiAutomacao`
  - Limpa dados antigos do banco (`ExecuteDeleteAsync`)
  - Insere novos controladores no banco local
  - Registra logs de sucesso/erro

- **`SincronizacaoBackground`** - Background service (herda de `BackgroundService`)
  - Executa sincronização em loop com intervalo configurável
  - Usa `SincronizacaoConfiguracao` para definir PainelId e timer
  - Trata erros e continua execução

- **`SincronizacaoConfiguracao`** - Configurações de sincronização
  - `PainelId` - ID do painel a sincronizar
  - `Automatica` - Flag para habilitar sincronização automática
  - `Agendamento.Timer` - Intervalo (padrão: 20 segundos)

### 4. Provider - Fornecimento de Dados

**`ProviderSincronizacao`** - Implementação de `IProviderSincronizacao`
- Lê dados do banco de dados local
- Retorna o **controlador master** (aquele com `Master == true`)
- Expõe métodos para obter:
  - Controlador master completo
  - Lista de dispositivos do controlador
  - Lista de módulos do controlador
- Usa mapeamento via `EntityModelMapper` para converter entidades do banco

### 5. Infrastructure - Infraestrutura

**Componentes:**

- **`SincronizacaoDbContext`** - DbContext do Entity Framework Core
  - Usa SQLite como banco de dados
  - Contém `DbSet<ControladorConfiguracao>`

- **`ApiAutomacao`** - Implementação de `IApiAutomacao`
  - Cliente HTTP para comunicação com API externa
  - Usa `HttpClient` configurado com `ApiConfiguracao`

- **`ControladorConfiguracao`** - Entidade de persistência
  - Mapeia `Controlador` para tabela do banco
  - Armazena dados serializados ou relacionais

### 6. Extensions - Configuração e DI

**Componentes:**

- **`DependencyInjectionConfig`** - Registro de serviços
  - `AddRegisterServices()` - Registra `ISincronizarControladores` e `IProviderSincronizacao` como Scoped

- **`Config`** - Configuração do módulo
  - Configura DbContext, HttpClient, etc.

- **`PackageConfig`** - Configuração de pacotes NuGet
  - Registra `AddHttpClient<IApiAutomacao, ApiAutomacao>()`

## Fluxo de Execução

### Sincronização Automática

```
SincronizacaoBackground (loop)
    ↓
SincronizarControladores.ExecutarAsync()
    ↓
IApiAutomacao.ObterControladoresPorPainelAsync()
    ↓ (API externa)
Lista<Controlador>
    ↓
SincronizacaoDbContext (limpa dados antigos)
    ↓
SincronizacaoDbContext (insere novos dados)
    ↓
Banco de dados local atualizado
```

### Leitura de Dados

```
Outro módulo
    ↓
IProviderSincronizacao.ObterControlador()
    ↓
ProviderSincronizacao
    ↓
SincronizacaoDbContext (consulta banco)
    ↓
Controlador master (com Módulos, Dispositivos, etc.)
```

## Configuração

### Configuração de API (appsettings.json)

```json
{
  "ApiConfiguracao": {
    "BaseUrl": "https://api.exemplo.com",
    "MediaType": "application/json",
    "TimeoutSeconds": 30
  }
}
```

### Configuração de Sincronização

```csharp
var configuracao = new SincronizacaoConfiguracao
{
    PainelId = Guid.Parse("..."),
    Automatica = true,
    Agendamento = new Agendamento
    {
        Timer = TimeSpan.FromSeconds(20)
    }
};
```

## Registro no DI Container

No `Program.cs` ou configuração de serviços:

```csharp
services.AddSincronizacao(configuration);
```

Isso registra:
- `ISincronizarControladores` → `SincronizarControladores`
- `IProviderSincronizacao` → `ProviderSincronizacao`
- `IApiAutomacao` → `ApiAutomacao` (via HttpClient)
- `SincronizacaoDbContext` (DbContext)

## Dependências

**NuGet Packages:**
- `Microsoft.EntityFrameworkCore` (10.0.9)
- `Microsoft.EntityFrameworkCore.Sqlite` (10.0.9)
- `Microsoft.Extensions.Http` (10.0.9)
- `Microsoft.Extensions.Hosting.Abstractions` (10.0.7)
- `Microsoft.Extensions.Options.ConfigurationExtensions` (10.0.9)
- `Microsoft.Extensions.Logging.Abstractions` (10.0.9)
- `SQLitePCLRaw.bundle_e_sqlite3` (3.0.3)

**Project References:**
- `Toolbox.Automacao.Core` - Contém `Result<T>` e outras abstrações compartilhadas

## Padrões Utilizados

- **Repository Pattern** - `ProviderSincronizacao` atua como repository para leitura
- **Factory Pattern** - (removido) anteriormente tinha `SincronizacaoFactory`
- **Background Service** - `SincronizacaoBackground` para execução periódica
- **Dependency Injection** - Todos os serviços são injetados via DI container
- **Options Pattern** - Configurações via `IOptions<ApiConfiguracao>`

## Considerações de Design

1. **Separação de responsabilidades** - Sincronização (escrita) e Provider (leitura) são separados
2. **Controlador Master** - O sistema assume que existe um controlador marcado como `Master`
3. **Sincronização completa** - Cada sincronização limpa todos os dados e reinsere (estratégia simples)
4. **Logs estruturados** - Usa `ILogger` para rastrear operações e erros
5. **CancellationToken** - Suporte a cancelamento em operações assíncronas

## Casos de Uso

1. **Sincronização inicial** - Ao iniciar a aplicação, o background service começa a sincronizar
2. **Leitura de configurações** - Outros módulos usam `IProviderSincronizacao` para ler controladores
3. **Atualização periódica** - A cada 20 segundos (configurável), dados são atualizados da API
4. **Tratamento de falhas** - Erros na API são logados mas não interrompem o loop
