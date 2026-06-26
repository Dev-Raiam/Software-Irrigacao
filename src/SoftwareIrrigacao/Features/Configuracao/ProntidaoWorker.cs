using Microsoft.EntityFrameworkCore;
using SoftwareIrrigacao.Data;
using SoftwareIrrigacao.Domain.Entity;
using SoftwareIrrigacao.Infrastructure.Cache;
using SoftwareIrrigacao.Shared.Constants;
using SoftwareIrrigacao.Shared.Contracts;
using SoftwareIrrigacao.Shared.State;

namespace SoftwareIrrigacao.Workers;

public class ProntidaoWorker : BackgroundService
{
    private readonly ApplicationStateManager _applicationStateManager;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICriptografia _criptografia;
    private readonly ILogger<ProntidaoWorker> _logger;
    private bool avisoEmitido = false;

    public ProntidaoWorker(
        ApplicationStateManager applicationStateManager,
        CredenciaisAplicacao credenciaisAplicacao,
        IServiceProvider serviceProvider,
        ICriptografia criptografia,
        ILogger<ProntidaoWorker> logger
    )
    {
        _applicationStateManager = applicationStateManager;
        _credenciaisAplicacao = credenciaisAplicacao;
        _serviceProvider = serviceProvider;
        _criptografia = criptografia;
        _logger = logger;
    }

    private static readonly TimeSpan IntervaloVerificacao = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(IntervaloVerificacao, stoppingToken);

                if (_credenciaisAplicacao.Invalida)
                {
                    if (!avisoEmitido)
                    {
                        _logger.LogInformation("Aguardando credenciais da aplicação...");
                        avisoEmitido = true;
                    }

                    var credenciais = await ObterCredenciais(stoppingToken);

                    if (credenciais is { Count: > 0 } && ContemCredenciais(credenciais))
                    {
                        _logger.LogInformation("Carregando credenciais do banco de dados...");
                        AdicionarCredenciaisAplicacao(credenciais);
                    }
                }

                if (!_credenciaisAplicacao.Invalida)
                {
                    _logger.LogInformation("Credenciais carregadas com sucesso. Aplicação pronta.");
                    _applicationStateManager.LiberarCredenciais();
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação do serviço");
                await Task.Delay(IntervaloVerificacao, stoppingToken);
            }
        }
    }

    private async Task<List<Configuracao>?> ObterCredenciais(CancellationToken cancellationToken)
    {
        var scoped = _serviceProvider.CreateScope();

        using var context = scoped.ServiceProvider.GetRequiredService<SoftwareIrrigacaoContext>();

        var credenciais = await context.Configuracoes.AsNoTracking().ToListAsync(cancellationToken);

        return credenciais;
    }

    private static bool ContemCredenciais(List<Configuracao> credenciais)
    {
        var chaves = new[]
        {
            ChavesBanco.Padrao.ContaId,
            ChavesBanco.Padrao.PainelId,
            ChavesBanco.Integracao.Chave,
            ChavesBanco.Integracao.Segredo,
            ChavesBanco.Integracao.ContextoId,
        };

        return chaves.All(chave => credenciais.Exists(c => c.Chave == chave));
    }

    private void AdicionarCredenciaisAplicacao(List<Configuracao> credenciais)
    {
        var contaId = Guid.Parse(
            credenciais.Find(c => c.Chave == ChavesBanco.Padrao.ContaId)!.Valor
        );
        var painelId = Guid.Parse(
            credenciais.Find(c => c.Chave == ChavesBanco.Padrao.PainelId)!.Valor
        );
        var integracaoChave = credenciais.Find(c => c.Chave == ChavesBanco.Integracao.Chave)!.Valor;
        var integracaoSegredo = credenciais
            .Find(c => c.Chave == ChavesBanco.Integracao.Segredo)!
            .Valor;
        var integracaoContextoId = Guid.Parse(
            credenciais.Find(c => c.Chave == ChavesBanco.Integracao.ContextoId)!.Valor
        );

        _credenciaisAplicacao.AdicionarConta(contaId);
        _credenciaisAplicacao.AdicionarPainel(painelId);
        _credenciaisAplicacao.AdicionarIntegracao(
            _criptografia.Descriptografar(integracaoChave),
            _criptografia.Descriptografar(integracaoSegredo),
            integracaoContextoId
        );
    }
}
