using IrrigacaoInteligente.Features.Configuracao.Credenciais;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
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
    private readonly ILogger<ConfiguracaoInicializacaoHandler> _logger;

    public ConfiguracaoInicializacaoHandler(
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        CredenciaisAplicacao credenciaisAplicacao,
        IServiceProvider serviceProvider,
        ILogger<ConfiguracaoInicializacaoHandler> logger
    )
        : base(uow)
    {
        _credenciaisAplicacao = credenciaisAplicacao;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ResponseResult> Handle(
        IniciarConfiguracaoInicializacao request,
        CancellationToken cancellationToken
    )
    {
        if (_credenciaisAplicacao.Invalida)
        {
            using var scope = _serviceProvider.CreateScope();
            var gerenciadorCredenciais =
                scope.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();

            await gerenciadorCredenciais.ObterContaId(cancellationToken);
            await gerenciadorCredenciais.ObterPainelId(cancellationToken);
            await gerenciadorCredenciais.ObterCredencialIntegracao(cancellationToken);
        }

        if (!_credenciaisAplicacao.Invalida)
        {
            _logger.LogInformation("Configurações Carregadas com Sucesso!!!");

            return Ok<ResponseResult>();
        }

        return NotFound();
    }
}
