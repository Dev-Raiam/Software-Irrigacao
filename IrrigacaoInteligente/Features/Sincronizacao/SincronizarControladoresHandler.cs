using IrrigacaoInteligente.Infrastructure.Data;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Roteadores.Sincronizacao;

public class SincronizarControladoresHandler
    : CommandHandler,
        ICommandHandler<SincronizarControladores>
{
    private readonly Features.Automacao.SincronizarAutomacao _sincronizarAutomacao;

    public SincronizarControladoresHandler(
        Features.Automacao.SincronizarAutomacao sincronizarAutomacao,
        IUnitOfWork<IrrigacaoInteligenteContext> uow
    )
        : base(uow)
    {
        _sincronizarAutomacao = sincronizarAutomacao;
    }

    public async Task<ResponseResult> Handle(
        SincronizarControladores request,
        CancellationToken cancellationToken
    )
    {
        await _sincronizarAutomacao.Executar(cancellationToken);

        return Ok<ResponseResult>();
    }
}
