namespace IrrigacaoInteligente.Features.Credenciais.Interfaces
{
    public interface ICriptografia
    {
        string Criptografar(string entrada);
        string Descriptografar(string entrada);
    }
}
