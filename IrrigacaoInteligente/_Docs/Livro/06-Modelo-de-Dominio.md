# Capítulo 6 — Modelo de Domínio

O domínio fica em `Domain/`, dividido em **Entities** (entidades) e **Enums** (enumerações). Ele é deliberadamente simples e sem dependências de infraestrutura.

## 6.1 Entidades

```
Domain/Entities/
├── Configuracao/              Configuracao
├── ConfiguracaoControlador/   Configuração específica de controlador
├── Hardware/                  ComandoAnalogico, ComandoDigital, Operacao
└── Telemetria/                Telemetria, TelemetriaResposta,
                               BlocoTelemetriaProtocolo, BlocoTelemetriaSinal
```

### 6.1.1 Hardware

As entidades de hardware modelam as duas naturezas de atuação:

- **`ComandoDigital`** — acionamentos liga/desliga (ex.: bomba, solenoide).
- **`ComandoAnalogico`** — valores contínuos (ex.: frequência de inversor, abertura proporcional).
- **`Operacao`** — representa uma operação sobre o hardware.

### 6.1.2 Telemetria

As entidades de telemetria modelam a leitura do campo:

- **`Telemetria`** — uma leitura associada a um controlador e dispositivo.
- **`TelemetriaResposta`** — formato de resposta vindo do hardware.
- **`BlocoTelemetriaProtocolo`** / **`BlocoTelemetriaSinal`** — blocos de baixo nível que descrevem como os dados de telemetria são estruturados no protocolo.

> **Nota:** No estado atual do código, `Telemetria.cs` está comentado por inteiro, refletindo uma refatoração. A forma conceitual da entidade era:
>
> ```csharp
> public class Telemetria
> {
>     public Guid Id { get; private set; }
>     public Guid ControladorId { get; private set; }
>     public Guid DispositivoId { get; private set; }
>     public string Descricao { get; private set; }
>     public DateTime CriadoEm { get; private set; }
>     public string? Dados { get; private set; }
> }
> ```

## 6.2 Enumerações

```
Domain/Enums/
├── DispositivoEnums.cs   DispositivoTipo, DispositivoUnidadeMedida
├── InterfaceEnums.cs
├── ModuloEnums.cs
└── PortaEnums.cs
```

### 6.2.1 DispositivoTipo

Cataloga os tipos de dispositivo com **códigos numéricos** e descrições legíveis via `[EnumMember]`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Domain\Enums\DispositivoEnums.cs:5-32
public enum DispositivoTipo : long
{
    [EnumMember(Value = "Sensor de tensão")]
    SensorTensao = 050101,

    [EnumMember(Value = "Sensor de corrente")]
    SensorCorrente = 050102,

    [EnumMember(Value = "Sensor de potência")]
    SensorPotencia = 050103,

    [EnumMember(Value = "Sensor de frequência")]
    SensorFrequencia = 050104,

    [EnumMember(Value = "Sensor de nível")]
    SensorNivel = 050201,

    [EnumMember(Value = "Sensor de pressão")]
    SensorPressao = 050202,

    [EnumMember(Value = "Sensor de boia")]
    SensorBoia = 050301,

    [EnumMember(Value = "Sensor de posição")]
    SensorPosicao = 050302,

    [EnumMember(Value = "Válvula solenoide")]
    ValvulaSolenoide = 0600,
```

Os códigos seguem uma hierarquia (ex.: `0501xx` para sensores elétricos, `0502xx` para sensores de fluido), o que facilita agrupar dispositivos por família.

### 6.2.2 DispositivoUnidadeMedida

Padroniza as unidades das leituras (`KPa`, `PSI`, `Bar`, `MPa`, `Celsius`, `Kelvin`, `Fahrenheit`, `Metro`, `Centimetro`, `Percentual`, `Volt`, `Ampere`). Isso evita ambiguidade na interpretação da telemetria em toda a cadeia nuvem ↔ borda ↔ hardware.

### 6.2.3 Demais enums

- **`InterfaceEnums`** — tipos de interface de comunicação.
- **`ModuloEnums`** — catálogo de módulos suportados.
- **`PortaEnums`** — tipos/estados de porta de conexão.

---

Anterior: [Capítulo 5 — Estrutura do Projeto](05-Estrutura-do-Projeto.md) · Próximo: [Capítulo 7 — Inicialização e Gates de Prontidão](07-Inicializacao-e-Gates.md)
