using System.Text.Json;
using WorkerService.Features.Automacao.Comandos;

namespace WorkerService.Features.Mensageria;

public class ProcessadorMensageria(IServiceProvider _serviceProvider)
{
    public async Task Processar(string payload, CancellationToken cancellationToken)
    {
        var comando = JsonSerializer.Deserialize<ComandoAcionar>(payload);

        var scope = _serviceProvider.CreateScope();
        var processadorComando = scope.ServiceProvider.GetRequiredService<ProcessarComando>();

        await processadorComando.Processar(comando!);
    }
}
