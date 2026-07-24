using Toolbox.Industrial.Core.Services.Modbus;
using Toolbox.Industrial.Driver.Tekon.Models;

namespace Toolbox.Industrial.Driver.Tekon.Interfaces
{
    public interface ITekonDriverFactory
    {
        ITekonDriver CriarDriver(TekonDriverConfig config);
    }
}
