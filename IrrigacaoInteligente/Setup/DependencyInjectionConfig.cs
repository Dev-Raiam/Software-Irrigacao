using System.Reflection;
using Autenticacao.Configurations;
using Autenticacao.Interfaces;
using Autenticacao.Services;
using IrrigacaoInteligente.Core.Cache;
using IrrigacaoInteligente.Core.Criptografia;
using IrrigacaoInteligente.Core.Mqtt;
using IrrigacaoInteligente.Core.State;
using IrrigacaoInteligente.Features.Configuracao;
using IrrigacaoInteligente.Workers;
using Microsoft.AspNetCore.DataProtection;
using MQTTnet;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Automacao.Sincronizacao.Interfaces;
using Toolbox.Core.Api.Configuration;

namespace IrrigacaoInteligente.Setup;

public static class DependencyInjectionConfig
{
    public static void AddRegisterServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));
        services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));
        services.AddHttpContextAccessor();
        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );

        var keysPath =
            configuration["DataProtection:KeysPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "Keys");

        services
            .AddDataProtection()
            .SetApplicationName("IrrigacaoInteligente")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

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
