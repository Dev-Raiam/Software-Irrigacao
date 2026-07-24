using System.Reflection;
using Toolbox.Automacao.Core.Messages;
using Toolbox.Automacao.Core.Messages.Integration;
using Toolbox.Core.Api.Configuration;
using Toolbox.Modulo.Tekon;
using Toolbox.Modulo.Tekon.Interfaces;

namespace SoftwareIrrigacao.Setup;

public static class DependencyInjectionConfig
{
    public static void AddRegisterServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<ITekonDispositivoFactory, TekonDispositivoFactory>();
        //services.AddSingleton<IModbusFactory, Modbus>();
        services.AddSingleton<ITekonDriverFactory, TekonDriverFactory>();
        //services.AddKeyedSingleton<IMqtt>(
        //    "local",
        //    (provider, key) =>
        //    {
        //        var factory = provider.GetRequiredService<IMqttFactory>();
        //        var config = provider.GetRequiredService<IOptions<MqttConfiguracao>>().Value;
        //        //Dados Contante
        //        var mqttConfig = new Configuration
        //        {
        //            Host = config.Servidor,
        //            Port = config.Porta,
        //            ClientId = Guid.NewGuid().ToString(),
        //            Username = config.Usuario,
        //            Password = config.Senha,
        //        };

        //        return factory.Criar(mqttConfig);
        //    }
        //);

        //services.AddKeyedSingleton<IMqtt>(
        //    "remoto",
        //    (provider, key) =>
        //    {
        //        // Obter dados de conexão no Edpoint na api autenticado !!!
        //        var factory = provider.GetRequiredService<IMqttFactory>();

        //        var mqttConfig = new Configuration
        //        {
        //            Host = "broker.freemqtt.com",
        //            Port = 1883,
        //            ClientId = Guid.NewGuid().ToString(),
        //            Username = "freemqtt",
        //            Password = "public",
        //        };

        //        return factory.Criar(mqttConfig);
        //    }
        //);

        //services.AddHostedService<WorkerTeste>();

        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(SincronizarAutomacao).GetTypeInfo().Assembly
        );

        services.AddScoped<CommandDispatcher>();
    }
}
