namespace SoftwareIrrigacao.Shared.State;

public class ApplicationStateManager
{
    private readonly TaskCompletionSource _credenciaisDefinidas = new();
    private readonly TaskCompletionSource _sincronizacaoConcluida = new();

    public void LiberarCredenciais() => _credenciaisDefinidas.TrySetResult();

    public void LiberarSincronizacao() => _sincronizacaoConcluida.TrySetResult();

    public Task AguardarCredenciaisAsync() => _credenciaisDefinidas.Task;

    public Task AguardarSincronizacaoAsync() => _sincronizacaoConcluida.Task;
}
