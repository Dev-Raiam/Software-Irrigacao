using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sincronizacao;

public class SincronizarPaineisHandler : CommandHandler, ICommandHandler<SincronizarPaineis>
{
    public SincronizarPaineisHandler(IUnitOfWork<WorkerServiceContext> uow)
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
