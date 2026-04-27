namespace WorkerService.Features.Mensageria.Remota;

public class ProcessarMensagemLocal
{
    public void Processar(string payload, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Mensagem local: {payload}");
    }
}
