using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Interfaces
{
    public interface ITekonDispositivoDado
    {
        string Modelo { get; }
        long? NumeroSerie { get; }
        IEnumerable<Metrica> ObterMetricas();
    }
}
