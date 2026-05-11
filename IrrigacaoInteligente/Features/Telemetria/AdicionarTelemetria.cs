using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Telemetria;

public class AdicionarTelemetria : Command
{
    public string Dados { get; init; } = null!;
}
