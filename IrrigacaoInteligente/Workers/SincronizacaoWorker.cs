using System;
using IrrigacaoInteligente.Core.Cache;
using IrrigacaoInteligente.Core.State;
using Toolbox.Automacao.Sincronizacao.Interfaces;

namespace IrrigacaoInteligente.Workers
{
    public class SincronizacaoWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SincronizacaoWorker> _logger;
        private readonly ApplicationStateManager _applicationStateManager;
        private readonly CredenciaisAplicacao _credenciaisAplicacao;
        private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;

        public SincronizacaoWorker(
            IServiceProvider serviceProvider,
            ILogger<SincronizacaoWorker> logger,
            ApplicationStateManager applicationStateManager,
            CredenciaisAplicacao credenciaisAplicacao,
            ArmazenamentoAutomacao armazenamentoAutomacao
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _applicationStateManager = applicationStateManager;
            _credenciaisAplicacao = credenciaisAplicacao;
            _armazenamentoAutomacao = armazenamentoAutomacao;
        }

        private static readonly TimeSpan timer = TimeSpan.FromSeconds(3);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _applicationStateManager.AguardarCredenciaisAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try 
                {
                    using var scoped = _serviceProvider.CreateScope();
                    var serviceSincronizar =
                        scoped.ServiceProvider.GetRequiredService<ISincronizarControladores>();
                    var dadosSincronizacao =
                        scoped.ServiceProvider.GetRequiredService<IDadosSincronizacao>();

                    await serviceSincronizar.ExecutarAsync(
                        _credenciaisAplicacao.PainelId,
                        stoppingToken
                    );

                    await AtualizarCacheAsync(dadosSincronizacao, stoppingToken);

                    _applicationStateManager.LiberarSincronizacao();

                    await Task.Delay(timer, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado na preparação do serviço");
                    await Task.Delay(timer, stoppingToken);
                }
            }
        }

        private async Task AtualizarCacheAsync(
            IDadosSincronizacao dadosSincronizacao,
            CancellationToken stoppingToken
        )
        {
            var controlador = await dadosSincronizacao.ObterControlador(stoppingToken);

            if (controlador is null)
            {
                _armazenamentoAutomacao.Limpar();
                return;
            }

            var modulos = await dadosSincronizacao.ObterModulos(stoppingToken);
            var dispositivos = await dadosSincronizacao.ObterDispositivos(stoppingToken);

            _armazenamentoAutomacao.Atualizar(controlador, modulos, dispositivos);
        }
    }
}
