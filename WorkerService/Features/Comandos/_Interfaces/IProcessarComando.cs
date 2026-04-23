using WorkerService.Features.Shared.Abstractions;

namespace WorkerService.Features.Comandos._Interfaces;

public interface IProcessarComando
{
    Task Processar(Comando comando);
}
