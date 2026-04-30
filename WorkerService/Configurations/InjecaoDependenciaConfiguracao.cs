using System.Reflection;
using MQTTnet;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Configuration;
using WorkerService.Features.Configuracao.Credenciais;
using WorkerService.Features.Configuracao.Credenciais.Interfaces;
using WorkerService.Features.Sincronizacao.Automacao;
using WorkerService.Features.Sincronizacao.Automacao.Interfaces;
using WorkerService.Infrastructure.Auth;
using WorkerService.Infrastructure.Criptografia;
using WorkerService.Infrastructure.Http;
using WorkerService.Infrastructure.Mqtt;
using WorkerService.State;
using WorkerService.Workers;

namespace WorkerService.Configurations;

public static class InjecaoDependenciaConfiguracao
{
    public static void RegistrarServicos(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguration"));
        services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));
        services.AddHttpContextAccessor();
        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );
        services.AddDataProtection();
        services.AddSingleton<ArmazenamentoToken>();
        services.AddSingleton<CredenciaisAplicacao>();
        services.AddSingleton<ArmazenamentoAutomacao>();

        services.AddSingleton<MqttClienteRemoto>(provider => new MqttClienteRemoto(
            "Remoto",
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));
        services.AddSingleton<MqttClienteLocal>(provider => new MqttClienteLocal(
            "Local",
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));

        services.AddSingleton<ConfiguracaoInicializacao>();
        services.AddSingleton<GerenciadorToken>();
        services.AddScoped<SincronizarAutomacao>();
        services.AddScoped<GerenciadorCredenciais>();
        services.AddScoped<ICriptografia, Criptografia>();
        services.AddTransient<ManipuladorTokenAcesso>();
        services.AddHostedService<ProntidaoWorker>();
        services.AddHostedService<SincronizacaoWorker>();
        services.AddHostedService<MqttWorker>();
        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services
            .AddHttpClient<IAutomacaoApi, AutomacaoApi>()
            .AddHttpMessageHandler<ManipuladorTokenAcesso>();
    }
}
