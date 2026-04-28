using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sincronizacao;

public class SincronizarModulosHandler : CommandHandler, ICommandHandler<SincronizarModulos>
{
    public SincronizarModulosHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        SincronizarModulos request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(SincronizarModulos)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
