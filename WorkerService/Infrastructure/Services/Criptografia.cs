using Microsoft.AspNetCore.DataProtection;
using WorkerService.Infrastructure.Interfaces;

namespace WorkerService.Infrastructure.Services;

public sealed class Criptografia(IDataProtectionProvider _provider) : ICriptografia
{
    private readonly IDataProtector _protector = _provider.CreateProtector(
        "Automacao.Credenciais.v1"
    );

    public string Criptografar(string entrada) => _protector.Protect(entrada);

    public string Descriptografar(string entrada) => _protector.Unprotect(entrada);
}
