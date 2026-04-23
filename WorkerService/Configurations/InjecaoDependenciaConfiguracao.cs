using MQTTnet;
using WorkerService.Features.Automacao.Comandos;
using WorkerService.Features.Automacao.Comandos.Executores;
using WorkerService.Features.Automacao.Sincronizacao;
using WorkerService.Features.Automacao.Sincronizacao.Dispositivos;
using WorkerService.Features.Automacao.Sincronizacao.InterfacesComunicacao;
using WorkerService.Features.Automacao.Sincronizacao.Paineis;
using WorkerService.Features.Automacao.Sincronizacao.Portas;
using WorkerService.Features.Configuracao.ConfiguracaoSistema;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais.Interfaces;
using WorkerService.Features.Configuracao.GerenciamentoToken;
using WorkerService.Features.Mensageria;
using WorkerService.Features.Prontidao;
using WorkerService.Features.Shared.Abstractions;
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

        services.AddSingleton<IMqttClient>(sp =>
        {
            var factory = new MqttClientFactory();
            return factory.CreateMqttClient();
        });
        services.AddSingleton<IMqttCliente, MqttCliente>();
        services.AddSingleton<Prontidao>();
        services.AddSingleton<GerenciadorToken>();

        services.AddScoped<SincronizarAutomacao>();
        services.AddScoped<ProcessarComando>();
        services.AddScoped<AcionarPorta>();
        services.AddScoped<GerenciadorCredenciais>();
        services.AddScoped<ICriptografia, Criptografia>();

        services.AddScoped<SincronizarAutomacao>();
        services.AddScoped<SincronizarPainel>();
        services.AddScoped<SincronizarDispositivos>();
        services.AddScoped<SincronizarPortas>();
        services.AddScoped<SincronizarInterfaces>();
        services.AddScoped<ConfigurarSistema>();

        services.AddTransient<ManipuladorTokenAcesso>();
        services.AddTransient<ProcessadorMensageria>();
    }
}
