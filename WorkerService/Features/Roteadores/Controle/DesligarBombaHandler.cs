using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Roteadores.Controle;

public class DesligarBombaHandler : CommandHandler, ICommandHandler<DesligarBomba>
{
    public DesligarBombaHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        DesligarBomba request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(DesligarBomba)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
