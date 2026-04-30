using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Features.Configuracao.Credenciais;
using WorkerService.Infrastructure.Data;
using WorkerService.State;

namespace WorkerService.Features.Configuracao.ConfiguracaoSistema;

public class IniciarConfiguracaoInicializacao() : Command;

public class ConfiguracaoInicializacaoHandler(
    IUnitOfWork<WorkerServiceContext> uow,
    CredenciaisAplicacao credenciaisAplicacao,
    IServiceProvider serviceProvider
) : CommandHandler(uow), ICommandHandler<IniciarConfiguracaoInicializacao>
{
    public async Task<ResponseResult> Handle(
        IniciarConfiguracaoInicializacao request,
        CancellationToken cancellationToken = default
    )
    {
        if (credenciaisAplicacao.Invalida)
        {
            using var scope = serviceProvider.CreateScope();
            var armazenamento = scope.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();

            await armazenamento.ObterPainelId(cancellationToken);
            await armazenamento.ObterCredencialIntegracao(cancellationToken);
            await armazenamento.ObterContaId(cancellationToken);
        }

        if (!credenciaisAplicacao.Invalida)
            return Ok<ResponseResult>();

        return NotFound();
    }
}
