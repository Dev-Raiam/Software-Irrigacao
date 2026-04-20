namespace WorkerService.Infrastructure.Interfaces;

public interface IProntidao
{
    Task<bool> PrepararAplicacaoAsync(CancellationToken cancellationToken);
    Task AguardarAsync(CancellationToken cancellationToken);
    void MarcarPronto();
}
