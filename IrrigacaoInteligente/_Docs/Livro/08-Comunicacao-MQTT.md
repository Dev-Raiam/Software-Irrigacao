# Capítulo 8 — Comunicação MQTT

## 8.1 Dois brokers, dois mundos

A comunicação MQTT é **bidirecional** e usa dois clientes distintos, ambos em `Infrastructure/Mqtt/`:

- **`MqttClienteRemoto`** — conecta ao broker da **nuvem** e recebe comandos.
- **`MqttClienteLocal`** — conecta ao broker **local** (junto ao hardware) e troca telemetria/comandos com o CLP.
- **`MqttCliente`** — classe base comum.

Ambos são registrados como **singletons**, criados a partir de uma `MqttClientFactory` do `MQTTnet`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:48-58
        services.AddSingleton<MqttClienteRemoto>(provider => new MqttClienteRemoto(
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));

        services.AddSingleton<MqttClienteLocal>(provider => new MqttClienteLocal(
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));
```

## 8.2 Tópicos

| Tópico | Broker | Direção | Descrição |
| ------ | ------ | ------- | --------- |
| `comando/{painelId}` | Remoto | Nuvem → Worker | Comandos vindos da nuvem |
| `telemetria/resposta` | Local | Hardware → Worker | Resposta de telemetria do hardware |

## 8.3 O `MqttWorker`

Esse worker espera o gate de MQTT e então estabelece as duas conexões. Ele primeiro aguarda a liberação:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Workers\MqttWorker.cs:39-44
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _aplicacao.AguardarLiberacaoMqtt(stoppingToken);

        using var scope = _serviceProvider.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<IrrigacaoInteligenteContext>();
```

### Conexão local

Usa os valores de `MqttConfiguracao` (servidor, porta, usuário, senha) lidos da configuração:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Workers\MqttWorker.cs:53-63
                if (!ConexaoLocalAtiva)
                {
                    await _mqttClienteLocal.ConectarAsync(
                        _mqttConfiguracao.Servidor,
                        _mqttConfiguracao.Porta,
                        Guid.NewGuid().ToString(),
                        _mqttConfiguracao.Usuario,
                        _mqttConfiguracao.Senha,
                        stoppingToken
                    );
                }
```

### Conexão remota

Atualmente o broker remoto está **hardcoded** (`broker.freemqtt.com:1883`, usuário `freemqtt` / `public`):

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Workers\MqttWorker.cs:65-75
                if (!ConexaoRemotaAtiva)
                {
                    await _mqttClienteRemoto.ConectarAsync(
                        "broker.freemqtt.com",
                        1883,
                        Guid.NewGuid().ToString(),
                        "freemqtt",
                        "public",
                        stoppingToken
                    );
                }
```

> **Recomendação:** mover host/porta/credenciais do broker remoto para configuração (`appsettings`/User Secrets), assim como já é feito para o broker local.

### Assinatura e callbacks

Quando o remoto conecta, assina o tópico de comando e registra callbacks de mensageria e desconexão:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Workers\MqttWorker.cs:77-101
                if (_mqttClienteRemoto.Conectado && !ConexaoRemotaAtiva)
                {
                    ConexaoRemotaAtiva = true;

                    await _mqttClienteRemoto.AssinarTopicoAsync(
                        "comando/03800edb-8dff-4e2b-9ad8-00f0af1cdebf",
                        stoppingToken
                    );

                    _mqttClienteRemoto.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteRemoto.ExecutarCallbackDesconectado(stoppingToken);
                }

                if (_mqttClienteLocal.Conectado && !ConexaoLocalAtiva)
                {
                    ConexaoLocalAtiva = true;

                    await _mqttClienteLocal.AssinarTopicoAsync(
                        "telemetria/resposta",
                        stoppingToken
                    );

                    _mqttClienteLocal.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteLocal.ExecutarCallbackDesconectado(stoppingToken);
                }
```

> O `painelId` no tópico de comando também está fixo no código (`03800edb-...`). Em produção, ele deveria vir da identidade do dispositivo/credenciais.

## 8.4 Roteamento de comandos

Quando uma mensagem chega no `comando/{painelId}`:

```
MqttClienteRemoto (callback de mensageria)
  → desserializa JSON polimórfico pelo campo $type
  → mediator.Execute((dynamic)comando)
      → Handler em Features/Hardware/Controle
          → reencaminha ao broker LOCAL → hardware/CLP
```

O payload usa o formato polimórfico do Newtonsoft.Json:

```json
{ "$type": "...Comando, Toolbox.Automacao.Irrigacao", "...": "..." }
```

## 8.5 Estados de conexão

O `MqttWorker` mantém flags (`ConexaoIniciada`, `ConexaoLocalAtiva`, `ConexaoRemotaAtiva`) e só considera a inicialização concluída quando **ambos** os brokers estão conectados, encerrando o loop de tentativa a cada 5s.

---

Anterior: [Capítulo 7 — Inicialização e Gates de Prontidão](07-Inicializacao-e-Gates.md) · Próximo: [Capítulo 9 — Telemetria e Modbus](09-Telemetria-e-Modbus.md)
