using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Features.Roteadores.Sensores;

public class LerSensorPressaoHandler : CommandHandler, ICommandHandler<LerSensorPressao>
{
    public LerSensorPressaoHandler(IUnitOfWork<WorkerServiceContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        LerSensorPressao request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(LerSensorPressao)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
