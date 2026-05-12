using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Telemetria;

public class SalvarTelemetria : Command
{
    public string Dados { get; init; } = null!;
}
