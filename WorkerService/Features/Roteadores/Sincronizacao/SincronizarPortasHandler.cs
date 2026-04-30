using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Roteadores.Sincronizacao;

public class SincronizarPortasHandler : CommandHandler, ICommandHandler<SincronizarPortas>
{
    public SincronizarPortasHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        SincronizarPortas request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(SincronizarPortas)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
