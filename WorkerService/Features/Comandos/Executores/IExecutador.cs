namespace WorkerService.Features.Comandos.Executores;

public interface IExecutador
{
    public Task Executar(ComandoAcionar comando);
}
