using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services
{
    public interface IProvedorDataSincronizacao
    {
        Task<Controlador?> ObterControlador(CancellationToken cancellationToken);
        Task<List<Modulo>> ObterModulos(CancellationToken cancellationToken);
    }
}
