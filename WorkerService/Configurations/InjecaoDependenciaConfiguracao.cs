using MQTTnet;
using WorkerService.Features.Comandos;
using WorkerService.Features.Comandos.Executores;
using WorkerService.Features.Shared;
using WorkerService.Features.Sincronizacao.Automacao;
using WorkerService.Features.Sincronizacao.Automacao.Dispositivos;
using WorkerService.Features.Sincronizacao.Automacao.Interfaces;
using WorkerService.Features.Sincronizacao.Automacao.Paineis;
using WorkerService.Features.Sincronizacao.Automacao.Portas;
using WorkerService.Infrastructure.Http;
using WorkerService.Infrastructure.Interfaces;
using WorkerService.Infrastructure.Mqtt;
using WorkerService.Infrastructure.Services;
using WorkerService.Workers;

namespace WorkerService.Configurations;

public static class InjecaoDependenciaConfiguracao
{
    public static void RegistrarServicos(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguracao"));

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
        services.AddSingleton<IProntidao, Prontidao>();
        services.AddSingleton<GerenciadorToken>();

        services.AddScoped<SincronizarAutomacao>();
        services.AddScoped<ProcessarComando>();
        services.AddScoped<AcionarPorta>();
        services.AddScoped<ArmazenamentoCredenciais>();
        services.AddScoped<ICriptografia, Criptografia>();

        services.AddScoped<SincronizarAutomacao>();
        services.AddScoped<SincronizarPainel>();
        services.AddScoped<SincronizarDispositivos>();
        services.AddScoped<SincronizarPortas>();
        services.AddScoped<SincronizarInterfaces>();

        services.AddTransient<ManipuladorTokenAcesso>();
        services.AddTransient<IProcessadorMensageria, ProcessadorMensageria>();
    }
}
