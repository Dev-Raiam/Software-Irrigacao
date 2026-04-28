using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sensores;

public class LerSensorTemperaturaHandler : CommandHandler, ICommandHandler<LerSensorTemperatura>
{
    public LerSensorTemperaturaHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorTemperatura request,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorTemperatura)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
