using IrrigacaoInteligente.Configurations;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IrrigacaoInteligente.Workers;

public class MqttWorker : BackgroundService
{
    private readonly MqttClienteRemoto _mqttClienteRemoto;
    private readonly MqttClienteLocal _mqttClienteLocal;
    private readonly ILogger<MqttWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MqttConfiguracao _mqttConfiguracao;
    private readonly ConfiguracaoInicializacao _configuracaoInicializacao;
    private bool ConexaoIniciada = false;
    private bool ConexaoLocalAtiva = false;
    private bool ConexaoRemotaAtiva = false;

    public MqttWorker(
        MqttClienteRemoto mqttClienteRemoto,
        MqttClienteLocal mqttClienteLocal,
        ILogger<MqttWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<MqttConfiguracao> mqttConfiguracao,
        ConfiguracaoInicializacao configuracaoInicializacao
    )
    {
        _mqttClienteRemoto = mqttClienteRemoto;
        _mqttClienteLocal = mqttClienteLocal;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _mqttConfiguracao = mqttConfiguracao.Value;
        _configuracaoInicializacao = configuracaoInicializacao;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _configuracaoInicializacao.AguardarConfiguracaoInicializacaoAsync(stoppingToken);

        using var scope = _serviceProvider.CreateScope();
        var _context = scope.ServiceProvider.GetRequiredService<IrrigacaoInteligenteContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (ConexaoIniciada)
                    break;

                await _mqttClienteLocal.ConectarAsync(
                    _mqttConfiguracao.Servidor,
                    _mqttConfiguracao.Porta,
                    Guid.NewGuid().ToString(),
                    _mqttConfiguracao.Usuario,
                    _mqttConfiguracao.Senha,
                    stoppingToken
                );

                await _mqttClienteRemoto.ConectarAsync(
                    "broker.freemqtt.com",
                    1883,
                    Guid.NewGuid().ToString(),
                    "freemqtt",
                    "public",
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
