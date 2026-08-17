using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Setup;

namespace Irrigacao.Atualizador;

public class Atualizador : BackgroundService
{
    private readonly IApiClient _client;
    private readonly IEntityStore _store;
    private readonly Updater _updater;
    private readonly ILogger<Atualizador> _logger;

    public Atualizador(
        IApiClient client,
        Updater updater,
        IEntityStore store,
        ILogger<Atualizador> logger
    )
    {
       
        _client = client;
        _updater = updater;
        _store = store;
        _logger = logger;
    }

    private string UrlAtualizacao =
        $"/automacao/v1/integracoes/{Application.IntegracaoId}/atualizacao-disponivel";

    private bool _containsRequisition = false;

    #region Repository

    private async Task<AtualizacaoDisponivel> ObterModeloRequest()
    {
        var contaId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ContaId);
        var painelId = await _store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
        var controladorId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ControladorId);
        var versaoAtual = await _store.FirstOrDefaultAsync<Configuracao>(x =>
            x.Id == Entity.Keys.VersaoAtual
        );

        return new AtualizacaoDisponivel(
            contaId,
            painelId,
            controladorId,
            null,
            versaoAtual.Valor.ToString()!,
            null,
            (int)RuntimeInformation.OSArchitecture
        );
    }

    #endregion
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AtualizacaoDisponivel? request = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_containsRequisition)
                {
                    var atualization = await CheckforUpdates(request!, stoppingToken);
                    if (atualization != null)
                        await _updater.Install(
                            atualization.UrlDownload,
                            stoppingToken
                        );
                }
                else
                {
                    if (Application.HasCredentials)
                    {
                        request = await ObterModeloRequest();
                        _containsRequisition = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na execução do serviço");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task<AtualizacaoResposta?> CheckforUpdates(
        AtualizacaoDisponivel request,
        CancellationToken cancellationToken
    )
    {
        var message = new HttpRequestMessage(HttpMethod.Query, UrlAtualizacao)
        {
            Content = JsonContent.Create(request),
        };

        var response = await _client.SendAsync<AtualizacaoResposta?>(message, cancellationToken);

        if (!response.Success)
        {
            _logger.LogWarning(response.Error);
            return null;
        }

        return response.Data;
    }
}
