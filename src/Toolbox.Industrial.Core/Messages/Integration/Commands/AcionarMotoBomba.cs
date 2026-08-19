namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarMotoBomba : RemoteCommand
    {
        public Guid MotoBombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
