using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Handlers.Sensores;

public class LerSensorUmidadeHandler : CommandHandler, ICommandHandler<LerSensorUmidade>
{
    public LerSensorUmidadeHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorUmidade request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorUmidade)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
