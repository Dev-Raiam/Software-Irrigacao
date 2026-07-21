namespace Toolbox.Automacao.Core.Application.Comandos
{
    public class AcionarBomba : CommandBase
    {
        public Guid BombaId { get; init; }
        public int VelocidadeRpm { get; set; } = 0;
    }
}
