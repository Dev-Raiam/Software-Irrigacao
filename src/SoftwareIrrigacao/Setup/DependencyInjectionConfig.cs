using Microsoft.Extensions.Options;
using SoftwareIrrigacao.Infrastructure.Cache;
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
        services.AddSingleton<IModbusFactory, ModbusFactory>();
        services.AddSingleton<ITekonDriverFactory, TekonDriverFactory>();
        services.AddSingleton<IMqttFactory, MqttFactory>();
        
        services.AddKeyedSingleton<IMqtt>(
            "local",
            (provider, key) =>
            {
                var factory = provider.GetRequiredService<IMqttFactory>();
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

        services.AddKeyedSingleton<IMqtt>(
            "remoto",
            (provider, key) =>
            {
                var factory = provider.GetRequiredService<IMqttFactory>();

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
