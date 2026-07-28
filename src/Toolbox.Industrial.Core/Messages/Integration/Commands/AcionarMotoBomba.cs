using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarMotoBomba : Command
    {
        public Guid MotoBombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
