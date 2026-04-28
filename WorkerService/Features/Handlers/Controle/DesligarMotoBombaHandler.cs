using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Controle;

public class DesligarMotoBombaHandler : CommandHandler, ICommandHandler<DesligarMotoBomba>
{
    public DesligarMotoBombaHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        DesligarMotoBomba request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(DesligarMotoBomba)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
