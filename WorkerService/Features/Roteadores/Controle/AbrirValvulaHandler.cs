using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Roteadores.Controle;

public class AbrirValvulaHandler : CommandHandler, ICommandHandler<AbrirValvula>
{
    public AbrirValvulaHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        AbrirValvula request,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"Executando {nameof(AbrirValvula)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
