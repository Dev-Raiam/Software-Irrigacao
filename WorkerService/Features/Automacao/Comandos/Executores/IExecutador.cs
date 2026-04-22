namespace WorkerService.Features.Automacao.Comandos.Executores;

public interface IExecutador
{
    public Task Executar(ComandoAcionar comando);
}
