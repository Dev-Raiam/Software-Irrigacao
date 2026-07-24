namespace Toolbox.Automacao.Core.Messages.Integration
{
    public class DesligarInversorFrequencia : CommandBase
    {
        public Guid InversorId { get; init; }
    }
}
