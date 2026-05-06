using IrrigacaoInteligente.Infrastructure.Data;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Sincronizacao;

public class SincronizarPortasHandler : CommandHandler, ICommandHandler<SincronizarPortas>
{
    public SincronizarPortasHandler(IUnitOfWork<IrrigacaoInteligenteContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        SincronizarPortas request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(SincronizarPortas)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
