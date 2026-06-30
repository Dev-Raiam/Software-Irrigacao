//using SoftwareIrrigacao.Infrastructure.Cache;
//using SoftwareIrrigacao.Shared.State;
//using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
//using Toolbox.Automacao.Sincronizacao.Provedor;

//namespace SoftwareIrrigacao.Workers
//{
//    public class SincronizacaoWorker : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly ILogger<SincronizacaoWorker> _logger;
//        private readonly ApplicationStateManager _applicationStateManager;
//        private readonly CredenciaisAplicacao _credenciaisAplicacao;
//        private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;

//        public SincronizacaoWorker(
//            IServiceProvider serviceProvider,
//            ILogger<SincronizacaoWorker> logger,
//            ApplicationStateManager applicationStateManager,
//            CredenciaisAplicacao credenciaisAplicacao,
//            ArmazenamentoAutomacao armazenamentoAutomacao
//        )
//        {
//            _serviceProvider = serviceProvider;
//            _logger = logger;
//            _applicationStateManager = applicationStateManager;
//            _credenciaisAplicacao = credenciaisAplicacao;
//            _armazenamentoAutomacao = armazenamentoAutomacao;
//        }

//        private static readonly TimeSpan IntervaloSincronizacao = TimeSpan.FromSeconds(3);

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            await _applicationStateManager.AguardarCredenciaisAsync();

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    using var scoped = _serviceProvider.CreateScope();

//                    var serviceSincronizar =
//                        scoped.ServiceProvider.GetRequiredService<Toolbox.Automacao.Sincronizacao.Services.Sync.SincronizarControladores>();

//                    var dadosSincronizacao =
//                        scoped.ServiceProvider.GetRequiredService<DadosSincronizacao>();

//                    await serviceSincronizar.ExecutarAsync(
//                        _credenciaisAplicacao.PainelId,
//                        stoppingToken
//                    );

//                    await AtualizarCacheAsync(dadosSincronizacao, stoppingToken);

//                    //_applicationStateManager.LiberarSincronizacao();

//                    await Task.Delay(IntervaloSincronizacao, stoppingToken);
//                }
//                catch (OperationCanceledException)
//                {
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Erro inesperado na prepara��o do servi�o");
//                    await Task.Delay(IntervaloSincronizacao, stoppingToken);
//                }
//            }
//        }

//        private async Task AtualizarCacheAsync(
//            DadosSincronizacao dadosSincronizacao,
//            CancellationToken cancellationToken
//        )
//        {
//            var controlador = await dadosSincronizacao.ObterControlador(cancellationToken);

//            if (controlador == null)
//            {
//                _armazenamentoAutomacao.Limpar();
//                return;
//            }

//            var ctrl = await dadosSincronizacao.ObterControlador(cancellationToken);
//            var dispositivos = await dadosSincronizacao.ObterDispositivos(cancellationToken);

//            Console.WriteLine($"Modulos: {ctrl.Modulos.Count()}");
//            Console.WriteLine($"Dispositivos: {dispositivos.Count}");

//            // _armazenamentoAutomacao.Atualizar(controlador, modulos, dispositivos);
//        }
//    }
//}
