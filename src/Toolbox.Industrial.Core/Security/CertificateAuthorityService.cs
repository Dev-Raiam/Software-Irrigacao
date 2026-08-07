using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Toolbox.Core.Extensions;
using Toolbox.Industrial.Core.Data;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Security;

internal interface ICertificateAuthorityService : IDisposable
{
    X509Certificate2 GetCertificate(string subject = "localhost");

    public Configuracao? GetCertificateStore(string subject = "localhost");

    void Save(X509Certificate2 certificate, string subject = "localhost");

    X509Certificate2 Sign(
        CertificateRequest request,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter
    );

    void Renew(string subject = "localhost", X509Certificate2? certificate = null);
}

internal sealed class CertificateAuthorityService : ICertificateAuthorityService
{
    public const string fileNameRootCA = "ca.crt";
    private readonly ConcurrentDictionary<string, X509Certificate2> _cache = new();
    private readonly ILogger<CertificateAuthorityService> _logger;
    private readonly object _sync = new();
    private readonly IEntityStore _store;

    public CertificateAuthorityService(
        IEntityStore store,
        ILogger<CertificateAuthorityService> logger
    )
    {
        _store = store;
        _logger = logger;
    }

    public X509Certificate2 GetCertificate(string subject = "localhost")
    {
        subject.ThrowIfNull(nameof(subject));
        lock (_sync)
        {
            return _cache.GetOrAdd(subject, _ => LoadOrCreate(subject));
        }
    }

    public Configuracao? GetCertificateStore(string subject = "localhost")
    {
        subject.ThrowIfNull(nameof(subject));
        var id = $"{Entity.Keys.Security.CertificateAuthority}{subject}".GetId();
        return _store.FirstOrDefault<Configuracao>(x => x.Id == id);
    }

    private X509Certificate2 LoadOrCreate(string subject)
    {
        var data = GetCertificateStore(subject)?.Valor as Certificate;
        if (data is null)
        {
            _logger.LogInformation("Creating Root Certificate Authority.");
            var certificate = CreateRootCertificate();
            Save(certificate);
            return certificate;
        }

        return X509CertificateLoader.LoadPkcs12(
            data.Content,
            data.Password,
            X509KeyStorageFlags.Exportable
        );
    }

    public X509Certificate2 Sign(
        CertificateRequest request,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter
    )
    {
        Span<byte> serial = stackalloc byte[16];

        RandomNumberGenerator.Fill(serial);
        var certificate = GetCertificate();
        return request.Create(certificate, notBefore, notAfter, serial);
    }

    public void Renew(string subject = "localhost", X509Certificate2? certificate = null)
    {
        lock (_sync)
        {
            if (_cache.TryGetValue(subject, out var oldCertificate))
            {
                _logger.LogInformation($"Renewing Root Certificate Authority.");
                certificate ??= CreateRootCertificate();
                Save(certificate, subject: subject);
                _cache[subject] = certificate;
                oldCertificate?.Dispose();
            }
        }
    }

    private X509Certificate2 CreateRootCertificate()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var request = new CertificateRequest(
            "CN=Toolbox Industrial Root CA",
            ecdsa,
            HashAlgorithmName.SHA256
        );

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: true,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true
            )
        );

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign,
                true
            )
        );

        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false)
        );

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddYears(500)
        );

        if (OperatingSystem.IsWindows())
        {
            certificate.FriendlyName = "Toolbox Industrial Root CA";
        }

        return certificate;
    }

    private void InstallRootCertificate(X509Certificate2 certificate)
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);

        store.Open(OpenFlags.ReadWrite);

        store.Add(certificate);
    }

    public void Save(X509Certificate2 certificate, string subject = "localhost")
    {
        var password = GeneratePassword();
        var content = certificate.Export(X509ContentType.Pfx, password);
        //File.WriteAllBytes("ca.pfx", pfx);
        CertificateExporter.ExportCertificate(certificate, fileNameRootCA);
        InstallRootCertificate(certificate);
        var config = new Certificate
        {
            Subject = subject,
            Content = content,
            Password = password,
            Thumbprint = certificate.Thumbprint,
            NotBefore = certificate.NotBefore,
            NotAfter = certificate.NotAfter,
            CreatedAt = DateTime.UtcNow,
        };

        var data = GetCertificateStore(subject);
        if (data == null)
        {
            _store
                .InsertAsync(
                    new Configuracao(
                        id: $"{Entity.Keys.Security.CertificateAuthority}{subject}".GetId(),
                        configuracao: config,
                        grupo: Grupo.Api,
                        tipo: Tipo.Seguranca
                    )
                )
                .GetAwaiter()
                .GetResult();

            return;
        }
        data.Atualizar(config);
        _store.UpdateAsync(data).GetAwaiter().GetResult();
    }

    private static string GeneratePassword()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public void Dispose()
    {
        foreach (var item in _cache)
        {
            item.Value?.Dispose();
        }
        _cache.Clear();
    }
}
