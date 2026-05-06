using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Configuracao;

public class ValidarEstadoAplicacao : Command { }

public class EstadoAplicacaoHandler : CommandHandler, ICommandHandler<ValidarEstadoAplicacao>
{
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EstadoAplicacaoHandler> _logger;

    public EstadoAplicacaoHandler(
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        CredenciaisAplicacao credenciaisAplicacao,
        ArmazenamentoAutomacao armazenamentoAutomacao,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<EstadoAplicacaoHandler> logger
    )
        : base(uow)
    {
        _credenciaisAplicacao = credenciaisAplicacao;
        _armazenamentoAutomacao = armazenamentoAutomacao;
        _mediator = mediator;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ResponseResult> Handle(
        ValidarEstadoAplicacao request,
        CancellationToken cancellationToken
    )
    {
        // if (_credenciaisAplicacao.Invalida)
        // {
        //     // using var scope = _serviceProvider.CreateScope();
        //     // var gerenciadorCredenciais =
        //     //     scope.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();

        //     // await gerenciadorCredenciais.ObterContaId(cancellationToken);
        //     // await gerenciadorCredenciais.ObterPainelId(cancellationToken);
        //     // await gerenciadorCredenciais.ObterCredencialIntegracao(cancellationToken);
        // }

        if (!_credenciaisAplicacao.Invalida)
        {
            _logger.LogInformation("Configurações Carregadas com Sucesso!!!");

            if (_armazenamentoAutomacao.Invalido)
                await _mediator.Execute(
                    new SincronizarControladores(),
                    cancellationToken: cancellationToken
                );
            return Ok<ResponseResult>();
        }

        return NotFound();
    }
}
