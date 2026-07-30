using Toolbox.Industrial.Driver.TekonBkp.Models;

namespace Toolbox.Industrial.Driver.TekonBkp.Interfaces
{
    public interface ITekonDispositivoDado
    {
        string Modelo { get; }
        long? NumeroSerie { get; }
        IEnumerable<Metrica> ObterMetricas();
    }
}
