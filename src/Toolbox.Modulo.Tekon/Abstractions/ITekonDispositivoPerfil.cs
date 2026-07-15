using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Abstractions
{
    public interface ITekonDispositivoPerfil
    {
        string Modelo { get; }
        ConfiguracaoLeitura? HoldingRegisters(int? index = null);
        ConfiguracaoLeitura? CoilRegisters(int? index = null);
        ITekonDispositivoDado Parse(
            DispositivoContextoLeitura context);
    }
}
