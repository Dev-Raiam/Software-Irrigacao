# Capítulo 16 — Guia de Extensão

Este capítulo é prático: como estender o sistema seguindo os padrões existentes.

## 16.1 Adicionar um novo comando de hardware

1. **Criar a classe de comando** em `Toolbox.Automacao.Irrigacao/Comandos/` (projeto referenciado). Ela carrega os dados necessários (ex.: `Id` do dispositivo, valor).
2. **Criar o handler** em `Features/Hardware/Controle/` (ou na categoria apropriada), implementando `ICommandHandler<SeuComando>`.
3. **No handler**, localize o dispositivo/porta em `ArmazenamentoAutomacao` e publique o `ComandoDigital`/`ComandoAnalogico` no broker local.
4. **Pronto** — o mediator registra o handler automaticamente, pois o assembly já é escaneado.

O registro automático cobre o assembly do projeto e o do `Toolbox.Automacao.Irrigacao`:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:28-31
        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );
```

### Esqueleto de um handler

```csharp
public class MeuComandoHandler : CommandHandler, ICommandHandler<MeuComando>
{
    private readonly MqttClienteLocal _mqttCliente;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly MqttConfiguracao _mqttConfiguracao;

    public MeuComandoHandler(
        MqttClienteLocal mqttCliente,
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        ArmazenamentoAutomacao armazenamento,
        IOptions<MqttConfiguracao> mqttConfiguracao) : base(uow)
    {
        _mqttCliente = mqttCliente;
        _armazenamento = armazenamento;
        _mqttConfiguracao = mqttConfiguracao.Value;
    }

    public async Task<ResponseResult> Handle(MeuComando request, CancellationToken ct = default)
    {
        var dispositivo = _armazenamento.Dispositivos.FirstOrDefault(d => d.Id == request.Id);
        if (dispositivo is null) return NotFound();

        var porta = _armazenamento.Portas
            .FirstOrDefault(p => p.DispositivoConectadoId == dispositivo.Id);
        if (porta is null) return NotFound();

        await _mqttCliente.PublicarAsync(
            _mqttConfiguracao.TopicoCmdLocal,
            ComandoDigital.Acionar(porta.EnderecoLogico!),
            ct);

        return Ok<ResponseResult>();
    }
}
```

## 16.2 Adicionar um novo sensor

1. Defina o tipo no enum `DispositivoTipo` (com `[EnumMember]` descritivo) e, se necessário, a unidade em `DispositivoUnidadeMedida`.
2. Crie um handler `LerSensorXxxHandler` em `Features/Hardware/Sensores/`.
3. Faça a leitura (via estado/telemetria/Modbus) e retorne um `ResponseResult`.

## 16.3 Adicionar uma nova configuração

1. Crie a classe de opções (ex.: `MinhaOpcao`).
2. Vincule-a no `RegistrarServicos`: `services.Configure<MinhaOpcao>(configuration.GetSection("MinhaOpcao"));`.
3. Adicione a seção correspondente no `appsettings.json`.

## 16.4 Ativar um worker desativado

Vários workers estão comentados na DI. Para ativar (ex.: o `TekonWorker`), descomente o registro:

```@d:\Desenvolvimento\Backend\SoftwareIrrigacao\IrrigacaoInteligente\Configurations\InjecaoDependenciaConfiguracao.cs:65-69
        services.AddHostedService<ProntidaoWorkerCopy>();
        //services.AddHostedService<ProntidaoWorker>();
        // services.AddHostedService<SincronizacaoWorker>();
        services.AddHostedService<MqttWorker>();
        //services.AddHostedService<TekonWorker>();
```

> Antes de ativar o `TekonWorker`, garanta que o controlador master esteja sincronizado e que o barramento Modbus esteja acessível (Capítulo 9).

## 16.5 Boas práticas ao estender

- **Uma fatia por funcionalidade:** mantenha comando + handler coesos; evite "services" horizontais.
- **Não bloqueie workers:** respeite `CancellationToken` e use `await` adequadamente.
- **Estado em memória para leitura rápida:** consulte `ArmazenamentoAutomacao` em vez do banco em caminhos quentes.
- **Logue eventos significativos** com Serilog, sem poluir (use níveis adequados).
- **Externalize segredos:** nunca faça hardcode de credenciais (lição do broker remoto atual).
- **Sincronize a documentação:** ao adicionar features, atualize este livro e o `Docs/README.md`.

---

Anterior: [Capítulo 15 — Logs e Observabilidade](15-Logs-e-Observabilidade.md) · Voltar ao [Índice](00-Indice.md)
