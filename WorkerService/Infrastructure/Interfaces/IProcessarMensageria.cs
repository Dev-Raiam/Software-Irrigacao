namespace WorkerService.Infrastructure.Interfaces;

public interface IProcessadorMensageria
{
    Task Processar(string payload, CancellationToken cancellationToken);
}
