namespace WorkerService.Features.Mensageria.Remota;

public class ProcessarMensagemRemota
{
    public void Processar(string payload, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Mensagem remota: {payload}");
    }
}
