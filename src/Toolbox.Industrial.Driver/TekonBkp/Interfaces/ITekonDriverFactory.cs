using Toolbox.Industrial.Core.Communication.Modbus;
using Toolbox.Industrial.Driver.TekonBkp.Models;

namespace Toolbox.Industrial.Driver.TekonBkp.Interfaces
{
    public interface ITekonDriverFactory
    {
        ITekonDriver CriarDriver(TekonDriverConfig config);
    }
}
