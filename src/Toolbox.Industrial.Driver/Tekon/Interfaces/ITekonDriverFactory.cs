using Toolbox.Industrial.Core.Communication.Modbus;
using Toolbox.Industrial.Driver.Tekon.Models;

namespace Toolbox.Industrial.Driver.Tekon.Interfaces
{
    public interface ITekonDriverFactory
    {
        ITekonDriver CriarDriver(TekonDriverConfig config);
    }
}
