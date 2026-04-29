using Toolbox.Core.Messages;

namespace WorkerService.Features.Configuracao.ConfiguracaoSistema;

public class AtualizarCredenciais : Command
{
    public Guid ContaId { get; init; }
    public Guid PainelId { get; init; }
}
