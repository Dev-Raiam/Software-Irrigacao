using IrrigacaoInteligente.Infrastructure.Data;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Sincronizacao;

public class SincronizarPaineisHandler : CommandHandler, ICommandHandler<SincronizarPaineis>
{
    public SincronizarPaineisHandler(IUnitOfWork<IrrigacaoInteligenteContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        SincronizarPaineis request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(SincronizarPaineis)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
