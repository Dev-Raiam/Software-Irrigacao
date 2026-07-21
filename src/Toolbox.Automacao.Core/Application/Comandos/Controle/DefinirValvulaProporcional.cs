namespace Toolbox.Automacao.Core.Application.Comandos
{
    public class DefinirValvulaProporcional : CommandBase
    {
        public Guid ValvulaId { get; init; }
        public int Abertura { get; set; } = 0;
    }
}
