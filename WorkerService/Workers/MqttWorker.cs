using Microsoft.EntityFrameworkCore;
using WorkerService.Configurations;
using WorkerService.Features.Prontidao;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Workers;

public class MqttWorker(
    ILogger<MqttWorker> _logger,
    IMqttCliente _mqttCliente,
    Prontidao _servicoProntidao,
    IServiceProvider _serviceProvider,
    TopicoConfiguracao _topicoConfiguracao
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _servicoProntidao.AguardarAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var scope = _serviceProvider.CreateScope();
                //Jogar essas Configuraçoens ou Para Variaveis de Ambientes ou API
                string broker = "test.mosquitto.org";
                int port = 1883;
                Guid clientId = Guid.NewGuid();

                await _mqttCliente.Conectar(
                    broker,
                    port,
                    clientId.ToString(),
                    null,
                    null,
                    stoppingToken
                );
                _logger.LogInformation("Conectado ao broker MQTT.");

                var _context = scope.ServiceProvider.GetRequiredService<WorkerServiceContext>();

                var dispositivosIds = await _context
                    .Dispositivos.Select(d => new { Guid = d.Id })
                    .ToListAsync(stoppingToken);

                await _mqttCliente.Assinar(_topicoConfiguracao.Topico, stoppingToken);

                foreach (var dispositivo in dispositivosIds)
                {
                    await _mqttCliente.Assinar(
                        $"dispositivo/{dispositivo.Guid}/comando",
                        stoppingToken
                    );
                }

                _logger.LogInformation($"Topicos assinados");

                _mqttCliente.IniciarMensageria(stoppingToken);

                _logger.LogInformation($"Mensageria Iniciada");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Encerramento normal
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao conectar ao broker MQTT. Nova tentativa em 10 segundos."
                );
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
