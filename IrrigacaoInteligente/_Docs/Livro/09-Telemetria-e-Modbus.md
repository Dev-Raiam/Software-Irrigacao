# Capítulo 9 — Telemetria e Modbus

## 9.1 Por que Modbus

O hardware industrial de campo (sensores, módulos de E/S) tipicamente fala **Modbus** — um protocolo serial/TCP simples e onipresente na automação. O IrrigacaoInteligente usa as bibliotecas `NModbus`, `NModbus.Serial` e `System.IO.Ports` para ler registradores e coils dos dispositivos.

## 9.2 O `TekonWorker`

A leitura de telemetria é feita pelo worker em `Workers/Telemetria/TekonWorker.cs`. O fluxo conceitual é:

```
TekonWorker
  → aguarda liberação da aplicação (AguardarLiberacaoAplicacao)
  → lê o controlador master de ArmazenamentoAutomacao
  → loop (a cada 2s):
      → ReadHoldingRegisters / ReadCoils (NModbus)
      → decodifica o modelo → telemetria
      → grava JSON em {Log:Path}/tekon.json
```

> **Estado atual:** o `TekonWorker` está **comentado** na configuração de DI (`//services.AddHostedService<TekonWorker>();`), ou seja, a leitura Modbus não está ativa na execução atual. Reative-o registrando o hosted service quando o hardware estiver disponível.

## 9.3 Entidades de telemetria

A telemetria é modelada em `Domain/Entities/Telemetria/`:

- **`Telemetria`** — leitura associada a controlador e dispositivo.
- **`TelemetriaResposta`** — formato da resposta vinda do hardware.
- **`BlocoTelemetriaProtocolo`** — descreve o bloco de dados no nível do protocolo.
- **`BlocoTelemetriaSinal`** — descreve o sinal/medida dentro do bloco.

A modelagem em "blocos" reflete como dispositivos Modbus organizam dados: blocos contíguos de registradores que precisam ser decodificados conforme o protocolo do fabricante (aqui, "Tekon").

## 9.4 Pipeline de leitura → publicação

```
[ Sensor Modbus ] --registers--> [ TekonWorker ] --decodifica--> [ Telemetria ]
                                                                       |
                                          +----------------------------+
                                          v                            v
                              grava {Log:Path}/tekon.json     (PublicarTelemetria via MQTT)
```

As features de telemetria ficam em `Features/Telemetria/`:

- **`PublicarTelemetria`** — publica a telemetria (ex.: via MQTT).
- **`SalvarTelemetria`** — persiste a telemetria.
- **`Tekon/TekonTelemetria`** — lógica específica de decodificação do protocolo Tekon.

## 9.5 Frequência e arquivo de saída

- O loop de leitura roda **a cada 2 segundos**.
- O resultado é gravado como JSON em `{Log:Path}/tekon.json`, servindo como snapshot do último estado lido — útil para depuração e para outros consumidores locais.

## 9.6 Boas práticas ao reativar o Modbus

- Garanta que o **controlador master** esteja corretamente sincronizado em `ArmazenamentoAutomacao` antes de iniciar a leitura.
- Trate timeouts de barramento serial com retry/backoff.
- Não bloqueie o loop: leituras Modbus podem ser lentas; mantenha o intervalo e o cancelamento cooperativo (`CancellationToken`).

---

Anterior: [Capítulo 8 — Comunicação MQTT](08-Comunicacao-MQTT.md) · Próximo: [Capítulo 10 — Hardware: Comandos e Sensores](10-Hardware-Comandos-e-Sensores.md)
