using System;
using IrrigacaoInteligente.Core.Cache;
using IrrigacaoInteligente.Core.State;
using Toolbox.Automacao.Sincronizacao.Interfaces;

namespace IrrigacaoInteligente.Workers
{
    public class SincronizacaoWorker : BackgroundService
    {
        private readonly TimeSpan timer = TimeSpan.FromSeconds(10);
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationStateManager _applicationStateManager;
        private readonly CredenciaisAplicacao _credenciaisAplicacao;

        public SincronizacaoWorker(
            IServiceProvider serviceProvider,
            ApplicationStateManager applicationStateManager,
            CredenciaisAplicacao credenciaisAplicacao
        )
        {
            _serviceProvider = serviceProvider;
            _applicationStateManager = applicationStateManager;
            _credenciaisAplicacao = credenciaisAplicacao;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _applicationStateManager.AguardarCredenciaisAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scoped = _serviceProvider.CreateScope();
                var serviceSincronizar =
                    scoped.ServiceProvider.GetRequiredService<ISincronizarControladores>();

                await Task.Delay(timer);
                await serviceSincronizar.ExecutarAsync(Guid.Parse(""), stoppingToken);
            }
        }
    }
}
