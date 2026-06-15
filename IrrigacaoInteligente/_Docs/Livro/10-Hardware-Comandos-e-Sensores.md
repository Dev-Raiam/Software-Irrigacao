# Capítulo 10 — Hardware: Comandos e Sensores

As funcionalidades de hardware ficam em `Features/Hardware/`, divididas em **Controle** (atuação) e **Sensores** (leitura). Cada arquivo é um *handler* CQRS — uma fatia vertical.

## 10.1 Controle (atuadores)

`Features/Hardware/Controle/` reúne os comandos de atuação:

| Handler | Ação |
| ------- | ---- |
| `AcionarBombaHandler` / `DesligarBombaHandler` | Liga/desliga bomba |
| `AcionarMotoBombaHandler` / `DesligarMotoBombaHandler` | Liga/desliga moto-bomba |
| `AcionarSolenoidHandler` / `DesligarSolenoidHandler` | Aciona/desliga válvula solenoide |
| `AcionarInversorFrequenciaHandler` / `DesligarInversorFrequenciaHandler` | Liga/desliga inversor |
| `DefinirFrequenciaInversorHandler` | Define a frequência do inversor (analógico) |
| `AbrirValvulaHandler` / `FecharValvulaHandler` | Abre/fecha válvula |
| `DefinirValvulaProporcionalHandler` | Define abertura proporcional (analógico) |

## 10.2 Anatomia de um handler de controle

O padrão de um handler de comando digital é: localizar o dispositivo e sua porta no estado sincronizado, então publicar o comando no broker local. Conceitualmente (do `AcionarBombaHandler`, hoje comentado durante refatoração):

```csharp
public async Task<ResponseResult> Handle(AcionarBomba request, CancellationToken ct = default)
{
    var dispositivo = _armazenamento.Dispositivos.FirstOrDefault(d => d.Id == request.Id);
    if (dispositivo is null)
        return NotFound();

    var porta = _armazenamento.Portas
        .Where(p => p.DispositivoConectadoId == dispositivo.Id)
        .FirstOrDefault();
    if (porta is null)
        return NotFound();

    await _mqttCliente.PublicarAsync(
        _mqttConfiguracao.TopicoCmdLocal,
        ComandoDigital.Acionar(porta.EnderecoLogico!),
        ct
    );

    return Ok<ResponseResult>();
}
```

Pontos-chave do padrão:

- **Resolução por estado em memória:** o handler usa `ArmazenamentoAutomacao` (dispositivos e portas sincronizados) — não consulta o banco a cada comando.
- **Endereço lógico:** o `EnderecoLogico` da porta diz ao hardware onde atuar.
- **Publicação local:** o comando vira um `ComandoDigital`/`ComandoAnalogico` publicado no broker local.
- **ResponseResult padronizado:** `Ok`, `NotFound`, etc., herdados de `CommandHandler` do `Toolbox.Core.Api`.

> **Nota:** Vários handlers dessa pasta estão atualmente comentados por inteiro, refletindo uma refatoração da camada de hardware. A estrutura acima descreve o comportamento pretendido.

## 10.3 Sensores (leitura)

`Features/Hardware/Sensores/` reúne os handlers de leitura:

| Handler | Mede |
| ------- | ---- |
| `LerSensorCorrenteHandler` | Corrente (Ampere) |
| `LerSensorTensaoHandler` | Tensão (Volt) |
| `LerSensorPressaoHandler` | Pressão (KPa/PSI/Bar/MPa) |
| `LerSensorNivelHandler` | Nível |
| `LerSensorTemperaturaHandler` | Temperatura (Celsius/Kelvin/Fahrenheit) |
| `LerSensorUmidadeHandler` | Umidade (Percentual) |
| `LerSensorDistanciaHandler` | Distância (Metro/Centimetro) |
| `LerSensorPhHandler` | pH |
| `LerMonitorPosicaoHandler` | Posição |

## 10.4 Comando digital vs. analógico

- **Digital:** estado discreto liga/desliga. Handlers de bomba, moto-bomba, solenoide e os de acionar/desligar inversor.
- **Analógico:** valor contínuo. `DefinirFrequenciaInversor` e `DefinirValvulaProporcional` carregam um valor numérico a ser escrito no hardware.

Essa distinção espelha as entidades `ComandoDigital` e `ComandoAnalogico` do domínio (Capítulo 6).

## 10.5 Como um comando chega até aqui

1. Nuvem publica em `comando/{painelId}` (broker remoto).
2. `MqttClienteRemoto` desserializa o JSON polimórfico pelo `$type`.
3. `mediator.Execute((dynamic)comando)` resolve o handler correto.
4. O handler publica o `ComandoDigital`/`ComandoAnalogico` no broker local.
5. O hardware/CLP executa fisicamente a ação.

---

Anterior: [Capítulo 9 — Telemetria e Modbus](09-Telemetria-e-Modbus.md) · Próximo: [Capítulo 11 — Persistência e Sincronização](11-Persistencia-e-Sincronizacao.md)
