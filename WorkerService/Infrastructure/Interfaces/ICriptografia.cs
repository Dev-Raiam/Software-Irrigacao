namespace WorkerService.Infrastructure.Interfaces
{
    public interface ICriptografia
    {
        string Criptografar(string entrada);
        string Descriptografar(string entrada);
    }
}
