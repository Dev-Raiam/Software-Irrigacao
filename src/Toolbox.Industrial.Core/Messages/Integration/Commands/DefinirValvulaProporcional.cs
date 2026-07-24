namespace Toolbox.Automacao.Core.Messages.Integration
{
    public class DefinirValvulaProporcional : CommandBase
    {
        public Guid ValvulaId { get; init; }
        public int Abertura { get; set; } = 0;
    }
}
