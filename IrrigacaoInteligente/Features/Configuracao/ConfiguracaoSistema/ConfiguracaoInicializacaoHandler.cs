using IrrigacaoInteligente.Features.Configuracao.Credenciais;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Configuracao.ConfiguracaoSistema;

public class IniciarConfiguracaoInicializacao : Command { }

public class ConfiguracaoInicializacaoHandler
    : CommandHandler,
        ICommandHandler<IniciarConfiguracaoInicializacao>
{
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private readonly IServiceProvider _serviceProvider;

    public ConfiguracaoInicializacaoHandler(
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        CredenciaisAplicacao credenciaisAplicacao,
        IServiceProvider serviceProvider
    )
        : base(uow)
    {
        _credenciaisAplicacao = credenciaisAplicacao;
        _serviceProvider = serviceProvider;
    }

    public async Task<ResponseResult> Handle(
        IniciarConfiguracaoInicializacao request,
        CancellationToken cancellationToken = default
    )
    {
        if (_credenciaisAplicacao.Invalida)
        {
            using var scope = _serviceProvider.CreateScope();
            var armazenamento = scope.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();

            await armazenamento.ObterPainelId(cancellationToken);
            await armazenamento.ObterCredencialIntegracao(cancellationToken);
            await armazenamento.ObterContaId(cancellationToken);
        }

        if (!_credenciaisAplicacao.Invalida)
            return Ok<ResponseResult>();

        return NotFound();
    }
}
