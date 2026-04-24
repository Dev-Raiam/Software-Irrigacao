using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WorkerService.Configurations;
using WorkerService.Features.Prontidao;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Workers;

public class MqttWorker(Func<string, IMqttCliente> _factoryMqttCliente, ILogger<MqttWorker> _logger)
    : BackgroundService
{
    public bool ConexaoIniciada { get; private set; } = false;
    private readonly IMqttCliente _mqttClienteNuvem = _factoryMqttCliente(InstanciasMqtt.Nuvem);
    private readonly IMqttCliente _mqttClienteLocal = _factoryMqttCliente(InstanciasMqtt.Local);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (ConexaoIniciada)
                    break;

                // string brokerLocal = "broker.emqx.io";
                // string brokerNuvem = "broker.freemqtt.com";
                string brokerLocal = "localhost";
                string brokerNuvem = "localhost";
                int port = 1883;
                Guid clientIdNuvem = Guid.NewGuid();
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

                await _mqttClienteNuvem.Conectar(
                    brokerNuvem,
                    port,
                    clientIdNuvem.ToString(),
                    // "freemqtt",
                    // "public",
                    null,
                    null,
                    stoppingToken
                );

                if (_mqttClienteNuvem.Conectado)
                {
                    _mqttClienteNuvem.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteNuvem.ExecutarCallbackDesconectado(stoppingToken);
                }
                if (_mqttClienteLocal.Conectado)
                {
                    _mqttClienteLocal.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteLocal.ExecutarCallbackDesconectado(stoppingToken);
                }
                if (_mqttClienteNuvem.Conectado && _mqttClienteLocal.Conectado)
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
