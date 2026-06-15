# Capítulo 7 — Inicialização e Gates de Prontidão

## 7.1 O problema da ordem de inicialização

Em um sistema de borda, vários workers precisam começar a operar — mas só **depois** que pré-condições estão satisfeitas (credenciais válidas, MQTT liberado). Iniciar tudo em paralelo, sem coordenação, causaria erros intermitentes. A solução adotada são os **gates de prontidão**.

## 7.2 A classe `Aplicacao`

O estado central de prontidão vive em `State/Aplicacao.cs`. Ela expõe **dois gates** baseados em `TaskCompletionSource`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\State\Aplicacao.cs:15-16
    private readonly TaskCompletionSource _pronto = new("Task-Aplicacao");
    private readonly TaskCompletionSource _prontoMqtt = new("Task-Aplicacao-Mqtt");
```

- **`_pronto`** — liberado por `LiberarAplicacao()`; workers que dependem da aplicação aguardam em `AguardarLiberacaoAplicacao()`.
- **`_prontoMqtt`** — liberado por `LiberarMqtt()`; o `MqttWorker` aguarda em `AguardarLiberacaoMqtt()`.

Os métodos do gate são simples e idempotentes (`TrySetResult` não lança se já liberado):

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\State\Aplicacao.cs:122-130
    public Task AguardarLiberacaoAplicacao(CancellationToken cancellationToken) =>
        _pronto.Task.WaitAsync(cancellationToken);

    public Task AguardarLiberacaoMqtt(CancellationToken cancellationToken) =>
        _prontoMqtt.Task.WaitAsync(cancellationToken);

    public void LiberarAplicacao() => _pronto.TrySetResult();

    public void LiberarMqtt() => _prontoMqtt.TrySetResult();
```

## 7.3 Validação do estado

`ValidarEstadoAplicacao` verifica, via mediator, se as credenciais estão prontas. Ele usa um *scope* de DI para resolver o `IMediator` e despachar o comando `ValidarEstadoAplicacao`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\State\Aplicacao.cs:41-60
    public async Task<bool> ValidarEstadoAplicacao(CancellationToken cancellationToken)
    {
        if (_pronto.Task.IsCompleted)
            return true;

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (!_avisoEstadoAplicacao && _credenciaisAplicacao.Invalida)
        {
            _logger.LogInformation("Aguardando configurações...");
            _avisoEstadoAplicacao = true;
        }

        var responseResult = await mediator.Execute(
            new ValidarEstadoAplicacao(),
            cancellationToken: cancellationToken
        );

        return responseResult.HttpStatusCode == HttpStatusCode.OK;
    }
```

> O bloco comentado logo abaixo (linhas 62–119) preserva uma lógica anterior mais ampla — que também liberava o MQTT, sincronizava dados e checava conexões. A versão ativa é enxuta: apenas valida credenciais.

## 7.4 O `ProntidaoWorker`

O worker de prontidão roda um loop a cada 5s. Quando o estado fica válido, registra "Aplicação pronta.", aguarda 1s e libera o gate da aplicação, encerrando o loop:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Workers\ProntidaoWorker.cs:16-44
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var aplicacaoEstado = await _aplicacao.ValidarEstadoAplicacao(stoppingToken);

                if (aplicacaoEstado)
                {
                    _logger.LogInformation("Aplicação pronta.");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    _aplicacao.LiberarAplicacao();

                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação do serviço");
            }
        }
    }
```

> **Atenção:** O worker efetivamente registrado na DI é o `ProntidaoWorkerCopy` (ver `InjecaoDependenciaConfiguracao`). O `ProntidaoWorker` mostrado aqui é a versão de referência; a cópia tende a conter variações em teste.

## 7.5 Fluxo completo de prontidão

```
ProntidaoWorker (loop 5s)
  → Aplicacao.ValidarEstadoAplicacao()
      → mediator.Execute(ValidarEstadoAplicacao)   (verifica credenciais no SQLite)
  → quando válido: LiberarAplicacao()   ← libera o gate _pronto
                                          (workers dependentes destravam)
```

---

Anterior: [Capítulo 6 — Modelo de Domínio](06-Modelo-de-Dominio.md) · Próximo: [Capítulo 8 — Comunicação MQTT](08-Comunicacao-MQTT.md)
