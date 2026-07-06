using MQTTnet;
using SoftwareIrrigacao.Infra.Cache;
using SoftwareIrrigacao.Infra.Mqtt;
using SoftwareIrrigacao.Shared.State;
using System.Reflection;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Configuration;

namespace SoftwareIrrigacao.Setup;

public static class DependencyInjectionConfig
{
    public static void AddRegisterServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        //services.AddScoped<ICredenciaisAutenticacao, CredenciaisAuthenticacao>();
        services.AddSingleton<CredenciaisAplicacao>();
        services.AddSingleton<ArmazenamentoAutomacao>();
        services.AddSingleton<ApplicationStateManager>();

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
