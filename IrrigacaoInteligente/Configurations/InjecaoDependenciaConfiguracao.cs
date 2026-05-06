using System.Reflection;
using IrrigacaoInteligente.Features.Automacao;
using IrrigacaoInteligente.Features.Automacao.Interfaces;
using IrrigacaoInteligente.Features.Credenciais;
using IrrigacaoInteligente.Features.Credenciais.Interfaces;
using IrrigacaoInteligente.Infrastructure.Auth;
using IrrigacaoInteligente.Infrastructure.Criptografia;
using IrrigacaoInteligente.Infrastructure.Http;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;
using IrrigacaoInteligente.Workers;
using Microsoft.AspNetCore.DataProtection;
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
        services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));
        services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));
        services.AddHttpContextAccessor();
        services.AddMediator(
            Assembly.GetExecutingAssembly(),
            typeof(AcionarBomba).GetTypeInfo().Assembly
        );
        services
            .AddDataProtection()
            .SetApplicationName("IrrigacaoInteligente")
            .PersistKeysToFileSystem(
                new DirectoryInfo(@"D:\Desenvolvimento\Backend\SoftwareIrrigacao")
            );

        services.AddSingleton<ArmazenamentoToken>();
        services.AddSingleton<CredenciaisAplicacao>();
        services.AddSingleton<ArmazenamentoAutomacao>();
        services.AddSingleton<ICriptografia, Criptografia>();
        services.AddScoped<GerenciadorCredenciais>();

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

        services.AddSingleton<Aplicacao>();
        services.AddSingleton<GerenciadorToken>();
        services.AddScoped<SincronizarAutomacao>();
        services.AddTransient<ManipuladorTokenAcesso>();
        services.AddHostedService<ProntidaoWorker>();
        // services.AddHostedService<SincronizacaoWorker>();
        services.AddHostedService<MqttWorker>();
        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services
            .AddHttpClient<IAutomacaoApi, AutomacaoApi>()
            .AddHttpMessageHandler<ManipuladorTokenAcesso>();
    }
}
