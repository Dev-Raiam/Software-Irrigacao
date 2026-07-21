using Toolbox.Automacao.Core.Application.Comandos;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Services;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace SoftwareIrrigacao.Features.Sincronizacao;

public class SincronizarAutomacaoHandler : ICommandHandler<SincronizarAutomacao>
{
    private readonly ISincronizacao _sincronizacao;
    private readonly IGerenciadorConfiguracao _gerenciadorConfiguracao;
    private readonly ILogger<SincronizarAutomacaoHandler> _logger;

    public SincronizarAutomacaoHandler(
        ISincronizacao sincronizacao,
        IGerenciadorConfiguracao gerenciadorConfiguracao,
        ILogger<SincronizarAutomacaoHandler> logger
    )
    {
        _sincronizacao = sincronizacao;
        _gerenciadorConfiguracao = gerenciadorConfiguracao;
        _logger = logger;
    }

    public async Task<ResponseResult> Handle(
        SincronizarAutomacao request,
        CancellationToken cancellationToken
    )
    {
        var painelId = _gerenciadorConfiguracao.ObterCredencialPainel();

        if (painelId == Guid.Empty)
        {
            _logger.LogWarning("Sincronização cancelada: PainelId não configurado");
            return ResponseResult.Result(System.Net.HttpStatusCode.BadRequest);
        }

        _logger.LogInformation("Iniciando sincronização para PainelId: {PainelId}", painelId);

        await _sincronizacao.SincronizarAutomacao(painelId, cancellationToken);

        _logger.LogInformation("Sincronização concluída para PainelId: {PainelId}", painelId);

        return ResponseResult.Result(System.Net.HttpStatusCode.OK);
    }
}
