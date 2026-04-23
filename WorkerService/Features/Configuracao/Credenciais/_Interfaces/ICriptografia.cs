namespace WorkerService.Features.Configuracao.Credenciais.Interfaces
{
    public interface ICriptografia
    {
        string Criptografar(string entrada);
        string Descriptografar(string entrada);
    }
}
