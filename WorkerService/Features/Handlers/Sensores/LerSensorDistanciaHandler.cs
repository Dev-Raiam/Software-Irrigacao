using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sensores;

public class LerSensorDistanciaHandler : CommandHandler, ICommandHandler<LerSensorDistancia>
{
    public LerSensorDistanciaHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorDistancia request,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorDistancia)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
