using WorkerService.Features.Shared;

namespace WorkerService.Features.Comandos.Executores;

public class AcionarPorta(ILogger<AcionarPorta> _logger) : IExecutador
{
    public Task Executar(ComandoAcionar comando)
    {
        _logger.LogInformation($"Acionando porta, {comando.IdentificadorPorta}");
        return Task.CompletedTask;
    }
}
