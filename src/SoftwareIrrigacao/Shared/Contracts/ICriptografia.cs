namespace SoftwareIrrigacao.Shared.Contracts
{
    public interface ICriptografia
    {
        string Criptografar(string entrada);
        string Descriptografar(string entrada);
    }
}
