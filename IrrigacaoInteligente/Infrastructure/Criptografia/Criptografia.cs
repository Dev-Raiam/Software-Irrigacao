using IrrigacaoInteligente.Features.Configuracao.Credenciais.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace IrrigacaoInteligente.Infrastructure.Criptografia;

public sealed class Criptografia : ICriptografia
{
    private readonly IDataProtector _protector;

    public Criptografia(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Automacao.Credenciais.v2");
    }

    public string Criptografar(string entrada) => _protector.Protect(entrada);

    public string Descriptografar(string entrada) => _protector.Unprotect(entrada);
}
