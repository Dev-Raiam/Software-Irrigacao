using Toolbox.Industrial.Driver.Tekon.Models;

namespace Toolbox.Industrial.Driver.Tekon.Interfaces
{
    public interface ITekonDispositivoDado
    {
        string Modelo { get; }
        long? NumeroSerie { get; }
        IEnumerable<Metrica> ObterMetricas();
    }
}
