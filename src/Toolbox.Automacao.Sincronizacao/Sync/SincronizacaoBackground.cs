using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Toolbox.Automacao.Sincronizacao.Core.Abstractions;

namespace Toolbox.Automacao.Sincronizacao.Sync
{
    internal class SincronizacaoBackground : BackgroundService
    {
        private readonly SincronizacaoConfiguracao _syncSetup;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SincronizacaoBackground> _logger;

        public SincronizacaoBackground(
            SincronizacaoConfiguracao syncSetup,
            IServiceProvider serviceProvider,
            ILogger<SincronizacaoBackground> logger
        )
        {
            _syncSetup = syncSetup;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scoped = _serviceProvider.CreateScope();
                    var sincronizarControladores = scoped.ServiceProvider.GetRequiredService<ISincronizarControladores>();

                    await sincronizarControladores.ExecutarAsync(
                        _syncSetup.PainelId,
                        stoppingToken
                    );

                    await Task.Delay(_syncSetup.Agendamento.Timer);
                }
                catch (Exception ex) 
                {
                    _logger.LogError("Error Inesperado {ex}", ex.Message);
                }
            }
        }
    }
}
