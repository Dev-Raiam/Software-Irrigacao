namespace WorkerService.Features.Configuracao.GerenciamentoCredenciais.Interfaces
{
    public interface ICriptografia
    {
        string Criptografar(string entrada);
        string Descriptografar(string entrada);
    }
}
