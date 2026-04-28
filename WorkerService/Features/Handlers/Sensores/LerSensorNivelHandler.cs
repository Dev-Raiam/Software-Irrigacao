using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sensores;

public class LerSensorNivelHandler : CommandHandler, ICommandHandler<LerSensorNivel>
{
    public LerSensorNivelHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorNivel request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorNivel)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
