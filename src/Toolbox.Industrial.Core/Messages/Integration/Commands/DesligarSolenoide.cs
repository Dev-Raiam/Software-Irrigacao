namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarSolenoide : Command
    {
        public Guid SolenoideId { get; init; }
    }
}
