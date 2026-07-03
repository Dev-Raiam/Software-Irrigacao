namespace Toolbox.Automacao.Core.Services;
public interface ICriptografia
{
    string Criptografar(string entrada);
    string Descriptografar(string entrada);
}