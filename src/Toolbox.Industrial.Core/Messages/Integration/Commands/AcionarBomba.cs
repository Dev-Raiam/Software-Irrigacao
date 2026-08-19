namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarBomba : RemoteCommand
    {
        public Guid BombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
