using SoftwareIrrigacao.Workers;

namespace SoftwareIrrigacao.Setup;

public static class WorkersConfig
{
    public static void AddRegisterWorkers(this IServiceCollection services)
    {
        services.AddHostedService<ProntidaoWorker>();
        //services.AddHostedService<SincronizacaoWorker>();
        services.AddHostedService<MqttWorker>();
        // services.AddHostedService<TekonWorker>();
    }
}
