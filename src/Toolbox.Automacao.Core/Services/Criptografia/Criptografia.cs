using Microsoft.AspNetCore.DataProtection;

namespace Toolbox.Automacao.Core.Services;
public interface ICriptografia
{
    string Criptografar(string entrada);
    string Descriptografar(string entrada);
}

internal sealed class Criptografia : ICriptografia
{
    private readonly IDataProtector _protector;

    public Criptografia(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Automacao.Credenciais.v2");
    }

    public string Criptografar(string entrada) => _protector.Protect(entrada);

    public string Descriptografar(string entrada) => _protector.Unprotect(entrada);
}
