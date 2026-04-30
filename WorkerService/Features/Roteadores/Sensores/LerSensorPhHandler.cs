using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Roteadores.Sensores;

public class LerSensorPhHandler : CommandHandler, ICommandHandler<LerSensorPh>
{
    public LerSensorPhHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorPh request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorPh)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
