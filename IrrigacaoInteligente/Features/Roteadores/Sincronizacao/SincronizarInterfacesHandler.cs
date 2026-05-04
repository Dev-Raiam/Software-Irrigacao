using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using IrrigacaoInteligente.Infrastructure.Data;

namespace IrrigacaoInteligente.Features.Roteadores.Sincronizacao;

public class SincronizarInterfacesHandler : CommandHandler, ICommandHandler<SincronizarInterfaces>
{
    public SincronizarInterfacesHandler(IUnitOfWork<IrrigacaoInteligenteContext> uow)
        : base(uow) { }

    public async Task<ResponseResult> Handle(
        SincronizarInterfaces request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(SincronizarInterfaces)}");
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
