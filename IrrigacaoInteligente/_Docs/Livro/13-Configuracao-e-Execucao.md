# Capítulo 13 — Configuração e Execução

## 13.1 Pré-requisitos

- **.NET 10.0**
- Broker MQTT **local** e **remoto** acessíveis
- **SQLite** (criado automaticamente via migrations + `SeedData`)

## 13.2 Arquivos de configuração

A aplicação carrega configuração de `appsettings.json`, do arquivo específico do ambiente e de User Secrets/variáveis de ambiente, nesta ordem:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:54-59
builder
    .Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();
```

Exemplo de configuração:

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

Essas seções são vinculadas a classes de opções na DI:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:25-26
        services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));
        services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));
```

## 13.3 Credenciais sensíveis

Credenciais **não** vão no `appsettings`. Elas são enviadas em runtime pelos endpoints HTTP e armazenadas criptografadas no SQLite:

| Método | Rota | Descrição | Proteções |
| ------ | ---- | --------- | --------- |
| `POST` | `/configuracao/credenciais` | Adiciona credenciais | Auth + `limite-tentativas` |
| `PUT` | `/configuracao/credenciais` | Atualiza credenciais | Auth + `limite-tentativas` |

Os endpoints são registrados em `Program.cs`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:87-88
AdicionarCredenciais.Endpoint(app);
AtualizarCredenciais.Endpoint(app);
```

Em desenvolvimento, também é possível usar **User Secrets** (há `UserSecretsId` no `.csproj`).

## 13.4 Diretório de trabalho

Logo no início, a aplicação fixa o diretório de trabalho no diretório do binário — importante para que caminhos relativos (logs, banco) funcionem igual em dev e em produção:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:9
Directory.SetCurrentDirectory(AppContext.BaseDirectory);
```

## 13.5 Executar em desenvolvimento

```bash
dotnet run
```

Ao subir, o sistema:

1. Configura o Serilog conforme o ambiente.
2. Registra serviços, workers e Data Protection.
3. Roda o `SeedData`.
4. Sobe os endpoints de credenciais.
5. Inicia os workers (prontidão e MQTT), que aguardam os gates.

Enquanto não houver credenciais válidas, o log mostrará "Aguardando configurações...". Após configurá-las via `POST /configuracao/credenciais`, o log mostrará "Aplicação pronta.".

## 13.6 Sequência de inicialização

```
Program.cs
  → Serilog
  → Configuração (appsettings + secrets + env)
  → DI (RegistrarContexto / RegistrarAuthenticacao / RegistrarServicos)
  → SeedData.Seed
  → UseAuthentication / UseAuthorization / UseRateLimiter
  → Endpoints de credenciais
  → app.RunAsync()  →  Workers começam e aguardam os gates
```

---

Anterior: [Capítulo 12 — Segurança](12-Seguranca.md) · Próximo: [Capítulo 14 — Deploy](14-Deploy.md)
