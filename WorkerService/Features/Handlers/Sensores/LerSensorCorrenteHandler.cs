using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sensores;

public class LerSensorCorrenteHandler : CommandHandler, ICommandHandler<LerSensorCorrente>
{
    public LerSensorCorrenteHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorCorrente request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorCorrente)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
