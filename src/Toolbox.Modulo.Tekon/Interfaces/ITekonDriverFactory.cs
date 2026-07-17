using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Models;

namespace Toolbox.Modulo.Tekon.Interfaces
{
    public interface ITekonDriverFactory
    {
        ITekonDriver CriarDriver(TekonDriverConfig config);
    }
}
