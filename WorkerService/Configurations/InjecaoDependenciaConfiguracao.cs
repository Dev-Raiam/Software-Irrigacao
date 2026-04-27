using MQTTnet;
using Toolbox.Core.Api.Configuration;
using WorkerService.Features.Configuracao.ConfiguracaoSistema;
using WorkerService.Features.Configuracao.Credenciais.Interfaces;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais;
using WorkerService.Features.Infrastructure.GerenciamentoToken;
using WorkerService.Features.Mensageria;
using WorkerService.Features.Mensageria.Remota;
using WorkerService.Features.Prontidao;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Features.Sincronizacao.Automacao;
using WorkerService.Infrastructure.Criptografia;
using WorkerService.Infrastructure.Http;
using WorkerService.Infrastructure.Mqtt;
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
        services.AddHttpContextAccessor();
        services.AddMediator(typeof(Program).Assembly);
        services.AddDataProtection();
        // //Configura o Serviço No Windows
        // services.AddWindowsService(options =>
        // {
        //     options.ServiceName = ".Net Worker Service Automacao";
        // });
        //WORKERS SERVICES
        services.AddHostedService<ProntidaoWorker>();
        services.AddHostedService<SincronizacaoWorker>();
        services.AddHostedService<MqttWorker>();

        services.AddWindowsService(options =>
        {
            options.ServiceName = "SoftwareAutomacao";
        });

        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services
            .AddHttpClient<IAutomacaoApi, AutomacaoApi>()
            .AddHttpMessageHandler<ManipuladorTokenAcesso>();

        services.AddSingleton<ArmazenamentoToken>();
        services.AddSingleton<IntegracaoConfiguracao>();
        services.AddSingleton<TopicoConfiguracao>();
        services.AddSingleton<ContaConfiguracao>();

        // Registrar as instâncias concretas
        services.AddSingleton<MqttClienteRemoto>(provider => new MqttClienteRemoto(
            "Remoto",
            new MqttClientFactory().CreateMqttClient(),
            provider.GetRequiredService<MqttClienteLocal>(),
            provider.GetRequiredService<ProcessarMensagemRemota>(),
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));

        services.AddSingleton<MqttClienteLocal>(provider => new MqttClienteLocal(
            "Local",
            new MqttClientFactory().CreateMqttClient(),
            provider.GetRequiredService<ProcessarMensagemLocal>(),
            provider.GetRequiredService<ILogger<MqttCliente>>()
        ));

        services.AddSingleton<Prontidao>();
        services.AddSingleton<GerenciadorToken>();

        services.AddScoped<SincronizarAutomacao>();
        services.AddScoped<GerenciadorCredenciais>();
        services.AddScoped<ICriptografia, Criptografia>();
        services.AddScoped<ConfigurarSistema>();

        services.AddTransient<ManipuladorTokenAcesso>();
        services.AddTransient<ProcessarMensagemRemota>();
        services.AddTransient<ProcessarMensagemLocal>();
    }
}
