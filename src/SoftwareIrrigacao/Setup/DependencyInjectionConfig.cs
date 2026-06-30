using MQTTnet;
using SoftwareIrrigacao.Infrastructure.Adapters;
using SoftwareIrrigacao.Infrastructure.Cache;
using SoftwareIrrigacao.Infrastructure.Criptografia;
using SoftwareIrrigacao.Infrastructure.Mqtt;
using SoftwareIrrigacao.Shared.Configuration;
using SoftwareIrrigacao.Shared.Contracts;
using SoftwareIrrigacao.Shared.State;
using System.Reflection;
using Toolbox.Automacao.Autenticacao;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Configuration;

namespace SoftwareIrrigacao.Setup;

public static class DependencyInjectionConfig
{
    public static void AddRegisterServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguracao"));
        services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));
        services.AddHttpContextAccessor();
        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );

        services.AddSingleton<CredenciaisAplicacao>();
        services.AddSingleton<ArmazenamentoAutomacao>();
        services.AddSingleton<ICriptografia, Criptografia>();

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

        services.AddSingleton<ApplicationStateManager>();
        services.AddTransient<AutenticacaoHandler>();

        services.AddScoped<ICredenciaisAutenticacao, CredenciaisAuthenticacao>();
    }
}
