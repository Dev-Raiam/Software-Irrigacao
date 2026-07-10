//using Microsoft.EntityFrameworkCore;
//using SoftwareIrrigacao.Infra.Cache;
//using SoftwareIrrigacao.Shared.State;
//using Toolbox.Automacao.Core.Data;
//using Toolbox.Automacao.Core.Services;

//namespace SoftwareIrrigacao.Workers;

//public class ProntidaoWorker : BackgroundService
//{
//    private readonly ApplicationStateManager _applicationStateManager;
//    private readonly CredenciaisAplicacao _credenciaisAplicacao;
//    private readonly IServiceProvider _serviceProvider;
//    private readonly IGerenciadorConfiguracao _configuracao;
//    private readonly ICriptografia _criptografia;
//    private readonly ILogger<ProntidaoWorker> _logger;
//    private bool avisoEmitido = false;

//    public ProntidaoWorker(
//        ApplicationStateManager applicationStateManager,
//        CredenciaisAplicacao credenciaisAplicacao,
//        IServiceProvider serviceProvider,
//        IGerenciadorConfiguracao configuracao,
//        ICriptografia criptografia,
//        ILogger<ProntidaoWorker> logger
//    )
//    {
//        _applicationStateManager = applicationStateManager;
//        _credenciaisAplicacao = credenciaisAplicacao;
//        _serviceProvider = serviceProvider;
//        _configuracao = configuracao;
//        _criptografia = criptografia;
//        _logger = logger;
//    }

//    private static readonly TimeSpan IntervaloVerificacao = TimeSpan.FromSeconds(5);

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                await Task.Delay(IntervaloVerificacao, stoppingToken);

//                if (_credenciaisAplicacao.Invalida)
//                {
//                    if (!avisoEmitido)
//                    {
//                        _logger.LogInformation("Aguardando credenciais da aplicação...");
//                        avisoEmitido = true;
//                    }

//                    var credenciais = _configuracao.ObterCredenciaisIntegracao();

//                    if (credenciais is { Count: > 0 } && ContemCredenciais(credenciais))
//                    {
//                        _logger.LogInformation("Carregando credenciais do banco de dados...");
//                        AdicionarCredenciaisAplicacao(credenciais);
//                    }
//                }

//                if (!_credenciaisAplicacao.Invalida)
//                {
//                    _logger.LogInformation("Credenciais carregadas com sucesso. Aplicação pronta.");
//                    _applicationStateManager.LiberarCredenciais();
//                    break;
//                }
//            }
//            catch (OperationCanceledException)
//            {
//                break;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Erro inesperado na preparação do serviço");
//                await Task.Delay(IntervaloVerificacao, stoppingToken);
//            }
//        }
//    }

//    //private async Task<List<Toolbox.Automacao.Core.Models.Configuracao>?> ObterCredenciais(
//    //    CancellationToken cancellationToken
//    //)
//    //{
//    //    var scoped = _serviceProvider.CreateScope();

//    //    using var context = scoped.ServiceProvider.GetRequiredService<IrrigacaoDbContext>();

//    //    var credenciais = await context
//    //        .Set<Toolbox.Automacao.Core.Models.Configuracao>()
//    //        .AsNoTracking()
//    //        .ToListAsync(cancellationToken);

//    //    return credenciais;
//    //}

//    private static bool ContemCredenciais(
//        List<Toolbox.Automacao.Core.Models.Configuracao> credenciais
//    )
//    {
//        var chaves = new[]
//        {
//            ChaveConfiguracao.Padrao.ContaId,
//            ChaveConfiguracao.Padrao.PainelId,
//            ChaveConfiguracao.Integracao.Chave,
//            ChaveConfiguracao.Integracao.Segredo,
//            ChaveConfiguracao.Integracao.ContextoId,
//        };

//        return chaves.All(chave => credenciais.Exists(c => c.Chave == chave));
//    }

//    private void AdicionarCredenciaisAplicacao(
//        List<Toolbox.Automacao.Core.Models.Configuracao> credenciais
//    )
//    {
//        var contaId = Guid.Parse(
//            credenciais.Find(c => c.Chave == ChaveConfiguracao.Padrao.ContaId)!.Valor
//        );
//        var painelId = Guid.Parse(
//            credenciais.Find(c => c.Chave == ChaveConfiguracao.Padrao.PainelId)!.Valor
//        );
//        var integracaoChave = credenciais.Find(c => c.Chave == ChaveConfiguracao.Integracao.Chave)!.Valor;
//        var integracaoSegredo = credenciais
//            .Find(c => c.Chave == ChaveConfiguracao.Integracao.Segredo)!
//            .Valor;
//        var integracaoContextoId = Guid.Parse(
//            credenciais.Find(c => c.Chave == ChaveConfiguracao.Integracao.ContextoId)!.Valor
//        );

//        _credenciaisAplicacao.AdicionarConta(contaId);
//        _credenciaisAplicacao.AdicionarPainel(painelId);
//        _credenciaisAplicacao.AdicionarIntegracao(
//            _criptografia.Descriptografar(integracaoChave),
//            _criptografia.Descriptografar(integracaoSegredo),
//            integracaoContextoId
//        );
//    }
//}
