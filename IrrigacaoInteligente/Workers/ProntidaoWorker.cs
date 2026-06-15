using IrrigacaoInteligente.Core.Cache;
using IrrigacaoInteligente.Core.Criptografia;
using IrrigacaoInteligente.Core.DataBase;
using IrrigacaoInteligente.Core.DataBase.Entity;
using IrrigacaoInteligente.Core.Shared.Utils;
using IrrigacaoInteligente.Core.State;
using Microsoft.EntityFrameworkCore;

namespace IrrigacaoInteligente.Workers;

public class ProntidaoWorker : BackgroundService
{
    private readonly ApplicationStateManager _applicationStateManager;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICriptografia _criptografia;
    private readonly ILogger<ProntidaoWorker> _logger;
    private bool avisoEmitido = false;

    public ProntidaoWorker(
        ApplicationStateManager applicationStateManager,
        CredenciaisAplicacao credenciaisAplicacao,
        ArmazenamentoAutomacao armazenamentoAutomacao,
        IServiceProvider serviceProvider,
        ICriptografia criptografia,
        ILogger<ProntidaoWorker> logger
    )
    {
        _applicationStateManager = applicationStateManager;
        _credenciaisAplicacao = credenciaisAplicacao;
        _armazenamentoAutomacao = armazenamentoAutomacao;
        _serviceProvider = serviceProvider;
        _criptografia = criptografia;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, stoppingToken);

                if (_credenciaisAplicacao.Invalida && !avisoEmitido)
                {
                    _logger.LogInformation("Aguardando Credenciais");
                    avisoEmitido = true;
                }

                if (_credenciaisAplicacao.Invalida)
                {
                    var credenciais = await ObterCredenciais(stoppingToken);

                    if (credenciais != null && credenciais.Count != 0)
                    {
                        _logger.LogInformation("Obtendo Credenciais do Banco");
                        
                        AdicionarCredenciaisAplicacao(credenciais);

                        _logger.LogInformation("Credenciais obtidas com sucesso");
                        
                        _applicationStateManager.LiberarCredenciais();

                    }
                    else
                    {
                        _logger.LogInformation("Credenciais não encontradas");
                        _applicationStateManager.LiberarCredenciais();
                        continue;
                    }
                }
                else 
                {
                    _logger.LogInformation("Credenciais obtidas com sucesso");
                }

                //if (_armazenamentoAutomacao.Invalido)
                //{
                //    _logger.LogInformation("Buscando controladores do banco de dados");

                //    var controladores = await ObterControladores(stoppingToken);

                //    if (controladores != null && controladores.Count == 0)
                //    {
                //        _logger.LogInformation("Adicionando controladores a aplicação");
                //        _armazenamentoAutomacao.Controladores.AddRange(controladores);
                //    }
                //    else
                //    {
                //        continue;
                //    }
                //}
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação do serviço");
            }
        }
    }

    //private async Task<List<Controlador>?> ObterControladores(CancellationToken cancellationToken)
    //{
    //    var scoped = _serviceProvider.CreateScope();

    //    using var context =
    //        scoped.ServiceProvider.GetRequiredService<IrrigacaoInteligenteContext>();

    //    var controladores = await context
    //        .ConfiguracoesControladores.AsNoTracking()
    //        .Select(cg => cg.Configuracao)
    //        .ToListAsync(cancellationToken);

    //    return controladores;
    //}

    private async Task<List<Configuracao>?> ObterCredenciais(CancellationToken cancellationToken)
    {
        var scoped = _serviceProvider.CreateScope();

        using var context =
            scoped.ServiceProvider.GetRequiredService<IrrigacaoInteligenteContext>();

        var credenciais = await context.Configuracoes.AsNoTracking().ToListAsync(cancellationToken);

        return credenciais;
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
