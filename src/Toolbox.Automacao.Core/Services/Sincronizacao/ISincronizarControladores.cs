namespace Toolbox.Automacao.Core.Services
{
    public interface ISincronizarControladores
    {
        Task ExecutarAsync(Guid PainelId, CancellationToken cancellationToken);
    }
}
