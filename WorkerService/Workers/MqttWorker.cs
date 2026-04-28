using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WorkerService.Configurations;
using WorkerService.Features.Prontidao;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;
using WorkerService.Infrastructure.Mqtt;

namespace WorkerService.Workers;

public class MqttWorker(
    MqttClienteRemoto _mqttClienteRemoto,
    MqttClienteLocal _mqttClienteLocal,
    ILogger<MqttWorker> _logger,
    IServiceProvider _serviceProvider
) : BackgroundService
{
    public bool ConexaoIniciada { get; private set; } = false;
    public List<Guid> DispositivosIds { get; private set; } = [];
    public bool ConexaoLocalAtiva { get; private set; } = false;
    public bool ConexaoRemotaAtiva { get; private set; } = false;
    public bool AvisoEmitido { get; private set; } = false;

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

                // string brokerLocal = "broker.emqx.io";
                // string brokerNuvem = "broker.freemqtt.com";
                string brokerLocal = "localhost";
                string brokerRemoto = "broker.freemqtt.com";
                int port = 1883;
                Guid clientIdRemoto = Guid.NewGuid();
                Guid clientIdLocal = Guid.NewGuid();

                await _mqttClienteLocal.Conectar(
                    brokerLocal,
                    port,
                    clientIdLocal.ToString(),
                    // "freemqtt",
                    // "public",
                    null,
                    null,
                    stoppingToken
                );
                await _mqttClienteRemoto.Conectar(
                    brokerRemoto,
                    port,
                    clientIdRemoto.ToString(),
                    "freemqtt",
                    "public",
                    // null,
                    // null,
                    stoppingToken
                );

                if (_mqttClienteRemoto.Conectado && !ConexaoRemotaAtiva)
                {
                    ConexaoRemotaAtiva = true;
                    /// Escutar Comandos da API MQTT
                    foreach (var dispositivoId in DispositivosIds)
                    {
                        await _mqttClienteRemoto.AssinarTopico(
                            $"comando/{dispositivoId}/api",
                            stoppingToken
                        );
                    }
                    _mqttClienteRemoto.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteRemoto.ExecutarCallbackDesconectado(stoppingToken);
                }
                if (_mqttClienteLocal.Conectado && !ConexaoLocalAtiva)
                {
                    ConexaoLocalAtiva = true;
                    await _mqttClienteRemoto.AssinarTopico("rapido", stoppingToken);
                    // foreach (var dispositivoId in DispositivosIds)
                    // {
                    //     await _mqttClienteLocal.AssinarTopico(
                    //         $"resposta/{dispositivoId}/api",
                    //         stoppingToken
                    //     );
                    // }
                    _mqttClienteLocal.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteLocal.ExecutarCallbackDesconectado(stoppingToken);
                }
                if (_mqttClienteRemoto.Conectado && _mqttClienteLocal.Conectado)
                {
                    ConexaoIniciada = true;
                }
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
