using Irrigacao.Atualizador.Extensions;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;

namespace Irrigacao.Atualizador
{
    public class Updater : BackgroundService
    {
        private readonly IEntityStore _store;
        private readonly IApiClient _client;
        private readonly IUpdateInstaller _installer;
        private readonly ILogger<Updater> _logger;

        public Updater(
            IEntityStore store,
            IApiClient client,
            IUpdateInstaller installer,
            ILogger<Updater> logger
        )
        {
            _store = store;
            _client = client;
            _installer = installer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var data = await _client.CheckUpdate(_store, _logger, stoppingToken);

                    if (data != null)
                        await _installer.Run(data, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado na execução do serviço");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
