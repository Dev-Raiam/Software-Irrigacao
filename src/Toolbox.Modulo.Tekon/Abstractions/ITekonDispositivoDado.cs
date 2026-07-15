using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Abstractions
{
    public interface ITekonDispositivoDado
    {
        string Modelo { get; }
        long? NumeroSerie { get; }
        IEnumerable<Metrica> ObterMetricas();
    }
}
