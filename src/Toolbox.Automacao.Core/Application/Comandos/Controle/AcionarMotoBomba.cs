namespace Toolbox.Automacao.Core.Application.Comandos
{
    public class AcionarMotoBomba : CommandBase
    {
        public Guid MotoBombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
