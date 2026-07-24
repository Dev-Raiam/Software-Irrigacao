namespace Toolbox.Automacao.Core.Messages.Integration
{
    public class AcionarMotoBomba : CommandBase
    {
        public Guid MotoBombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
