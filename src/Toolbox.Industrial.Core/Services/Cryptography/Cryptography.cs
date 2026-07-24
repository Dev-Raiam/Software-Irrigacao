using Microsoft.AspNetCore.DataProtection;

namespace Toolbox.Automacao.Core.Services.Cryptography;

public interface ICryptography
{
    string Encrypt(string value);
    string Decrypt(string value);
}

internal sealed class Cryptography : ICryptography
{
    private readonly IDataProtector _protector;

    public Cryptography(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("DataProtection.v1");
    }

    public string Encrypt(string value) => _protector.Protect(value);

    public string Decrypt(string value) => _protector.Unprotect(value);
}
