namespace Toolbox.Automacao.Core.Messages.Integration
{
    public class AcionarBomba : CommandBase
    {
        public Guid BombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
