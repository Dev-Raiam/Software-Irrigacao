using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Roteadores.Controle;

public class FecharValvulaHandler : CommandHandler, ICommandHandler<FecharValvula>
{
    public FecharValvulaHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        FecharValvula request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(FecharValvula)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
