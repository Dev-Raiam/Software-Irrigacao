using System.Text.Json;
using WorkerService.Features.Comandos;
using WorkerService.Infrastructure.Interfaces;

namespace WorkerService.Infrastructure.Mqtt;

public class ProcessadorMensageria(IServiceProvider _serviceProvider) : IProcessadorMensageria
{
    public async Task Processar(string payload, CancellationToken cancellationToken)
    {
        var comando = JsonSerializer.Deserialize<ComandoAcionar>(payload);

        var scope = _serviceProvider.CreateScope();
        var processadorComando = scope.ServiceProvider.GetRequiredService<ProcessarComando>();

        await processadorComando.Processar(comando!);
    }
}
