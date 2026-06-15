# Capítulo 15 — Logs e Observabilidade

## 15.1 Serilog

O logging é estruturado, feito com **Serilog**, configurado diretamente em `Program.cs`. O caminho base dos arquivos vem de `Log:Path`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:13
var logBasePath = builder.Configuration["Log:Path"];
```

## 15.2 Desenvolvimento vs. Produção

A configuração difere por ambiente:

- **Desenvolvimento:** escreve no **Console** e em **arquivo**.

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:15-33
if (builder.Environment.IsDevelopment())
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override(
            "Microsoft.EntityFrameworkCore",
            Serilog.Events.LogEventLevel.Warning
        )
        .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            $"{logBasePath}/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7
        )
        .CreateLogger();
}
```

- **Produção:** escreve **somente em arquivo** (sem Console), e também silencia `Microsoft.Hosting.Lifetime` para reduzir ruído.

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:35-52
else
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override(
            "Microsoft.EntityFrameworkCore",
            Serilog.Events.LogEventLevel.Warning
        )
        .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.File(
            $"{logBasePath}/log-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7
        )
        .CreateLogger();
}
```

## 15.3 Rotação de arquivos

- **Arquivo base:** `{Log:Path}/log-.txt` (o Serilog insere a data no nome).
- **Rotação:** diária (`RollingInterval.Day`).
- **Retenção:** os últimos **7** arquivos (`retainedFileCountLimit: 7`).

## 15.4 Filtros de nível (ruído)

Para manter os logs legíveis, vários namespaces da Microsoft são elevados para `Warning`:

- `Microsoft.AspNetCore`
- `Microsoft.EntityFrameworkCore`
- `System.Net.Http`
- `Microsoft.Hosting.Lifetime` (apenas em produção)

O nível mínimo geral é `Information`.

## 15.5 Encerramento limpo

No `finally` do bootstrap, os logs são liberados (flush) para não perder mensagens ao encerrar:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Program.cs:90-97
try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
```

## 15.6 Mensagens-chave para monitorar

| Mensagem | Significado |
| -------- | ----------- |
| `Aguardando configurações...` | Sem credenciais válidas ainda |
| `Aplicação pronta.` | Gate da aplicação liberado |
| `Erro inesperado na preparação do serviço` | Falha no `ProntidaoWorker` |
| `Erro inesperado na preparação do MQTT` | Falha no `MqttWorker` |

## 15.7 Telemetria como observabilidade local

Além dos logs, o `TekonWorker` (quando ativo) grava o último snapshot de telemetria em `{Log:Path}/tekon.json`, útil para inspeção rápida do estado do campo (ver Capítulo 9).

---

Anterior: [Capítulo 14 — Deploy](14-Deploy.md) · Próximo: [Capítulo 16 — Guia de Extensão](16-Guia-de-Extensao.md)
