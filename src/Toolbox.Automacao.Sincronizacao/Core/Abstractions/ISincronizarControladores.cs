namespace Toolbox.Automacao.Sincronizacao.Core.Abstractions
{
    public interface ISincronizarControladores
    {
        Task ExecutarAsync(Guid PainelId,CancellationToken cancellationToken);
    }
}
