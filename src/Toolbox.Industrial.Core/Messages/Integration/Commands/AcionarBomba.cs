using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarBomba : Command
    {
        public Guid BombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
