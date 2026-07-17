using MQTTnet;
using SoftwareIrrigacao.Features.Telemetria.Tekon;
using SoftwareIrrigacao.Infra.Cache;
using SoftwareIrrigacao.Infra.Mqtt;
using SoftwareIrrigacao.Shared.State;
using System.Reflection;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Configuration;
using Toolbox.Modulo.Tekon;
using Toolbox.Modulo.Tekon.Interfaces;

namespace SoftwareIrrigacao.Setup;

public static class DependencyInjectionConfig
{
    public static void AddRegisterServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<CredenciaisAplicacao>();
        services.AddSingleton<ArmazenamentoAutomacao>();
        services.AddSingleton<ApplicationStateManager>();

        services.AddSingleton<ITekonDispositivoFactory, TekonDispositivoFactory>();
        services.AddSingleton<IModbusFacadeFactory, ModbusFacadeFactory>();
        services.AddSingleton<ITekonDriverFactory, TekonDriverFactory>();

        services.AddHostedService<WorkerTeste>();

        services.AddSingleton<MqttClienteRemoto>(provider => new MqttClienteRemoto(
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));
        services.AddSingleton<MqttClienteLocal>(provider => new MqttClienteLocal(
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));

        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );
    }
}
