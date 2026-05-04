using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkerService.Configurations;
using WorkerService.Infrastructure.Data;
using WorkerService.Infrastructure.Mqtt;
using WorkerService.State;

namespace WorkerService.Workers;

public class MqttWorker(
    MqttClienteRemoto _mqttClienteRemoto,
    MqttClienteLocal _mqttClienteLocal,
    ILogger<MqttWorker> _logger,
    IServiceProvider _serviceProvider,
    IOptions<MqttConfiguracao> _mqttConfig
) : BackgroundService
{
    private readonly MqttConfiguracao _config = _mqttConfig.Value;
    private bool ConexaoIniciada = false;
    private List<Guid> DispositivosIds = [];
    private bool ConexaoLocalAtiva = false;
    private bool ConexaoRemotaAtiva = false;
    private bool AvisoEmitido = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _serviceProvider.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<WorkerServiceContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (ConexaoIniciada)
                    break;

                if (DispositivosIds.Count > 0)
                {
                    await Task.Delay(5000, stoppingToken);

                    DispositivosIds = await _context
                        .Dispositivos.AsNoTracking()
                        .Select(d => d.Id)
                        .ToListAsync(stoppingToken);

                    if (DispositivosIds.Count == 0 && !AvisoEmitido)
                    {
                        _logger.LogWarning(
                            "Nenhum dispositivo encontrado nova tentativa a 5 segundos..."
                        );

                        AvisoEmitido = true;
                    }
                    if (DispositivosIds.Count > 0)
                    {
                        _logger.LogInformation("Dispositivos encontrados");
                    }
                    return;
                }

                await _mqttClienteLocal.ConectarAsync(
                    _config.BrokerLocal,
                    _config.Porta,
                    Guid.NewGuid().ToString(),
                    null,
                    null,
                    stoppingToken
                );

                await _mqttClienteRemoto.ConectarAsync(
                    _config.BrokerRemoto,
                    _config.Porta,
                    Guid.NewGuid().ToString(),
                    _config.UsuarioRemoto,
                    _config.SenhaRemoto,
                    stoppingToken
                );

                if (_mqttClienteRemoto.Conectado && !ConexaoRemotaAtiva)
                {
                    ConexaoRemotaAtiva = true;

                    await _mqttClienteRemoto.AssinarTopicoAsync(
                        "comando/03800edb-8dff-4e2b-9ad8-00f0af1cdebf",
                        stoppingToken
                    );

                    _mqttClienteRemoto.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteRemoto.ExecutarCallbackDesconectado(stoppingToken);
                }

                if (_mqttClienteLocal.Conectado && !ConexaoLocalAtiva)
                {
                    ConexaoLocalAtiva = true;

                    // await _mqttClienteLocal.AssinarTopicoAsync(
                    //     "comando/03800edb-8dff-4e2b-9ad8-00f0af1cdebf",
                    //     stoppingToken
                    // );

                    _mqttClienteLocal.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteLocal.ExecutarCallbackDesconectado(stoppingToken);
                }

                if (_mqttClienteRemoto.Conectado && _mqttClienteLocal.Conectado)
                {
                    ConexaoIniciada = true;
                }
                // if (_mqttClienteRemoto.Conectado)
                // {
                //     ConexaoIniciada = true;
                // }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação do MQTT");
            }
        }
    }
}
