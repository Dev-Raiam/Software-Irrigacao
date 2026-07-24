namespace Toolbox.Automacao.Core.Messages.Integration
{
    public class DesligarSolenoide : CommandBase
    {
        public Guid SolenoideId { get; init; }
    }
}
