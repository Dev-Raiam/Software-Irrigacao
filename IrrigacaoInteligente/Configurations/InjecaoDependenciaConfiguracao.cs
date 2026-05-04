using System.Reflection;
using IrrigacaoInteligente.Features.Configuracao.Credenciais;
using IrrigacaoInteligente.Features.Configuracao.Credenciais.Interfaces;
using IrrigacaoInteligente.Features.Sincronizacao.Automacao;
using IrrigacaoInteligente.Features.Sincronizacao.Automacao.Interfaces;
using IrrigacaoInteligente.Infrastructure.Auth;
using IrrigacaoInteligente.Infrastructure.Criptografia;
using IrrigacaoInteligente.Infrastructure.Http;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;
using IrrigacaoInteligente.Workers;
using MQTTnet;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Configuration;

namespace IrrigacaoInteligente.Configurations;

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
            new MqttClientFactory().CreateMqttClient(),
            provider,
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));
        services.AddSingleton<MqttClienteLocal>(provider => new MqttClienteLocal(
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
