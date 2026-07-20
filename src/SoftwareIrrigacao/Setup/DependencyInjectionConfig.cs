using Microsoft.Extensions.Options;
using SoftwareIrrigacao.Infrastructure.Cache;
using SoftwareIrrigacao.Shared.State;
using System.Reflection;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Automacao.Core.Services.Mqtt;
using Toolbox.Automacao.Core.Setup;
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
        services.AddSingleton<IMqttFacadeFactory, MqttFacadeFactory>();
        
        services.AddKeyedSingleton<IMqttFacade>(
            "local",
            (provider, key) =>
            {
                var factory = provider.GetRequiredService<IMqttFacadeFactory>();
                var config = provider.GetRequiredService<IOptions<MqttConfiguracao>>().Value;

                var mqttConfig = new MqttConfig
                {
                    Host = config.Servidor,
                    Port = config.Porta,
                    ClientId = Guid.NewGuid().ToString(),
                    Username = config.Usuario,
                    Password = config.Senha,
                };

                return factory.Criar(mqttConfig);
            }
        );

        services.AddKeyedSingleton<IMqttFacade>(
            "remoto",
            (provider, key) =>
            {
                var factory = provider.GetRequiredService<IMqttFacadeFactory>();

                var mqttConfig = new MqttConfig
                {
                    Host = "broker.freemqtt.com",
                    Port = 1883,
                    ClientId = Guid.NewGuid().ToString(),
                    Username = "freemqtt",
                    Password = "public",
                };

                return factory.Criar(mqttConfig);
            }
        );

        //services.AddHostedService<WorkerTeste>();

        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );
    }
}
