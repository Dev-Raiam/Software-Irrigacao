using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Industrial.Core.Data;
using static Toolbox.Industrial.Core.Security.Certificate;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Security;

public interface ICertificateService
{
    X509Certificate2 GetCertificate();

    bool IsExpired();

    bool NeedsRenew();

    void Renew();
}

internal sealed class CertificateService : ICertificateService, IDisposable
{
    private readonly ICertificateAuthorityService _authorityService;
    private readonly ILogger<CertificateService> _logger;
    private const int RenewBeforeExpirationDays = 90;
    private X509Certificate2? _certificate;
    private readonly object _sync = new();
    private readonly IEntityStore _store;
    private readonly Purpose _purpose;
    private bool _disposed;

    public CertificateService(
        Purpose purpose,
        IEntityStore store,
        ILogger<CertificateService> logger,
        ICertificateAuthorityService authorityService
    )
    {
        _store = store;
        _logger = logger;
        _purpose = purpose;
        _authorityService = authorityService;
    }

    public X509Certificate2 GetCertificate()
    {
        ThrowIfDisposed();

        if (_certificate is not null)
            return _certificate;

        lock (_sync)
        {
            _certificate ??= LoadOrCreate();

            return _certificate;
        }
    }

    public bool IsExpired() => DateTime.UtcNow >= GetCertificate().NotAfter;

    public bool NeedsRenew() =>
        GetCertificate().NotAfter <= DateTime.UtcNow.AddDays(RenewBeforeExpirationDays);

    public void Renew()
    {
        ThrowIfDisposed();

        lock (_sync)
        {
            _logger.LogInformation($"Renewing {_purpose} certificate.");
            var certificate = CertificateFactory.Create(_purpose, _authorityService);
            Save(GetId(), certificate);

            var old = _certificate;
            _certificate = certificate;
            old?.Dispose();
        }
    }

    private Guid GetId() =>
        _purpose switch
        {
            Purpose.MqttLocal => Entity.Keys.Security.CertificateMqttLocal,
            Purpose.MqttRemoto => Entity.Keys.Security.CertificateMqttRemoto,
            Purpose.HttpsLocal => Entity.Keys.Security.CertificateHttpsLocal,
            _ => throw new NotSupportedException($"Unsupported certificate purpose: {_purpose}"),
        };

    private X509Certificate2 LoadOrCreate()
    {
        var id = GetId();

        var data = _store.FirstOrDefault<Configuracao>(x => x.Id == id)?.Valor as Certificate;

        if (data is null)
        {
            _logger.LogInformation($"Creating {_purpose} certificate.");

            var certificate2 = CertificateFactory.Create(_purpose, _authorityService);
            Save(id, certificate2);

            return certificate2;
        }
        var certificate = X509CertificateLoader.LoadPkcs12(
            data.Content,
            data.Password,
            X509KeyStorageFlags.Exportable
        );

        if (certificate.NotAfter <= DateTime.UtcNow.AddDays(RenewBeforeExpirationDays))
        {
            _logger.LogInformation($"{_purpose} certificate will expire soon.");
            certificate.Dispose();

            certificate = CertificateFactory.Create(_purpose, _authorityService);
            Save(id, certificate);
        }

        return certificate;
    }

    private void Save(Guid id, X509Certificate2 certificate)
    {
        var password = GeneratePassword();

        var content = certificate.Export(X509ContentType.Pfx, password);
        if (_purpose == Purpose.MqttLocal)
        {
            //File.WriteAllBytes("certificate.pfx", content);
            // Checar se a maquina contem o broker mqtt instalado (service)
            CertificateExporter.Export(certificate, _purpose.ToString().ToLowerInvariant());
            //TODO: Configuração de acesso ao Certificado no linux
            // Parar serviço do mosquitto (Linux/Windows)
            // Criar e configurar o arquivo local.conf (Linux/Windows)
            // Criar permisões dos certificados para linux para o usuario mosquitto acessar esse arquivo
            // Restart o mosquitto
        }
        var config = new Certificate
        {
            Content = content,
            Password = password,
            Thumbprint = certificate.Thumbprint,
            NotBefore = certificate.NotBefore,
            NotAfter = certificate.NotAfter,
            CreatedAt = DateTime.UtcNow,
        };
        Task.Run(() =>
                _store.UpsertAsync(
                    new Configuracao(
                        id: id,
                        configuracao: config,
                        grupo: Grupo.Api,
                        tipo: Tipo.Seguranca
                    )
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

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_sync)
        {
            if (_disposed)
                return;

            _certificate?.Dispose();

            _disposed = true;
        }
    }
}
