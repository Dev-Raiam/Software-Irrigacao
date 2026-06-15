namespace IrrigacaoInteligente.Core.Criptografia
{
    public interface ICriptografia
    {
        string Criptografar(string entrada);
        string Descriptografar(string entrada);
    }
}
