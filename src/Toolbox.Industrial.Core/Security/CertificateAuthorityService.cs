using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Toolbox.Industrial.Core.Data;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Security;

internal interface ICertificateAuthorityService
{
    X509Certificate2 GetCertificate();
    X509Certificate2 Sign(
        CertificateRequest request,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter
    );

    //X509Certificate2 Sign(
    //    CertificateRequest request,
    //    DateTimeOffset notBefore,
    //    DateTimeOffset notAfter
    //);
}

internal sealed class CertificateAuthorityService : ICertificateAuthorityService
{
    private readonly ILogger<CertificateAuthorityService> _logger;
    private X509Certificate2? _certificate;
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

    public X509Certificate2 GetCertificate()
    {
        if (_certificate is not null)
            return _certificate;

        lock (_sync)
        {
            _certificate ??= LoadOrCreate();
            return _certificate;
        }
    }

    private X509Certificate2 LoadOrCreate()
    {
        var data =
            _store
                .FirstOrDefault<Configuracao>(x =>
                    x.Id == Entity.Keys.Security.CertificateAuthority
                )
                ?.Valor as Certificate;

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

        return request.Create(GetCertificate(), notBefore, notAfter, serial);
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
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(20)
        );

        certificate.FriendlyName = "Toolbox Industrial Root CA";

        return certificate;
    }

    private void InstallRootCertificate(X509Certificate2 certificate)
    {
        using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);

        store.Open(OpenFlags.ReadWrite);

        store.Add(certificate);
    }

    private void Save(X509Certificate2 certificate)
    {
        var password = GeneratePassword();
        var content = certificate.Export(X509ContentType.Pfx, password);
        //File.WriteAllBytes("ca.pfx", pfx);
        CertificateExporter.ExportCertificate(certificate, "ca.crt");
        InstallRootCertificate(certificate);
        var config = new Certificate
        {
            Content = content,
            Password = password,
            Thumbprint = certificate.Thumbprint,
            NotBefore = certificate.NotBefore,
            NotAfter = certificate.NotAfter,
            CreatedAt = DateTime.UtcNow,
        };

        _store
            .UpsertAsync(
                new Configuracao(
                    id: Entity.Keys.Security.CertificateAuthority,
                    configuracao: config,
                    grupo: Grupo.Api,
                    tipo: Tipo.Seguranca
                )
            )
            .GetAwaiter()
            .GetResult();
    }

    private static string GeneratePassword()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
