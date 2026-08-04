using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Communication.Api;
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

internal record MqttConfig(
    int ListenerPort,
    bool AllowAnonymous,
    string CaFile,
    string CertFile,
    string KeyFile,
    bool RequireCertificate,
    bool UseIdentityAsUsername,
    string TlsVersion
);

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

        if (_purpose == Purpose.MqttLocal)
        {
            //File.WriteAllBytes("certificate.pfx", content);
            Task.Run(async () =>
                {
                    const string serviceName = "mosquitto";

                    bool existsService = await ExistsService(serviceName);

                    if (existsService)
                    {
                        CertificateExporter.Export(
                            certificate,
                            _purpose.ToString().ToLowerInvariant()
                        );

                        await AddPermissionsCertificates();
                        await StopService(serviceName);

                        CreateMqttConfFile();

                        await StartService(serviceName);
                    }
                })
                .GetAwaiter()
                .GetResult();
        }
    }

    private async Task<bool> ExistsService(string serviceName)
    {
        using var process = new Process();

        if (OperatingSystem.IsWindows())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sc",
                ArgumentList = { "query", serviceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                ArgumentList = { "status", serviceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }
        else
        {
            return false;
        }

        try
        {
            process.Start();
            await process.WaitForExitAsync();

            if (OperatingSystem.IsLinux())
            {
                // systemctl status: 0=ativo, 3=não existe, 4=inativo
                return process.ExitCode != 3;
            }

            // sc query: 0=serviço existe, !=0=não encontrado
            var output = await process.StandardOutput.ReadToEndAsync();
            return process.ExitCode == 0
                && output.Contains(serviceName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private async Task StartService(string serviceName)
    {
        using var process = new Process();

        if (OperatingSystem.IsWindows())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sc",
                ArgumentList = { "start", serviceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                ArgumentList = { "start", serviceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        process.Start();
        var code = process.ExitCode;
        await process.WaitForExitAsync();
    }

    private async Task StopService(string serviceName)
    {
        using var process = new Process();

        if (OperatingSystem.IsWindows())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sc",
                ArgumentList = { "stop", serviceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                ArgumentList = { "stop", serviceName },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task AddPermissionsCertificates()
    {
        const string user = "mosquitto";

        var crt = $"{_purpose}.crt".ToLowerInvariant();
        var key = $"{_purpose}.key".ToLowerInvariant();

        if (OperatingSystem.IsLinux())
        {
            await Chmod(crt, "644");
            await Chmod(CertificateAuthorityService.fileNameRootCA, "644");
            await Chmod(key, "600");

            await Chown(CertificateAuthorityService.fileNameRootCA, user, user);
            await Chown(crt, user, user);
            await Chown(key, user, user);

            // -rw-r--r-- mosquitto mosquitto mqttlocal.crt
            // -rw-r--r-- mosquitto mosquitto ca.crt
            // -rw-r--r-- mosquitto mosquitto mqttlocal.key
        }
    }

    private async Task Chmod(string filePath, string permissions)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                ArgumentList = { permissions, filePath },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task Chown(string filePath, string user, string? group = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chown",
                ArgumentList = { group != null ? $"{user}:{group}" : user, filePath },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    private void CreateMqttConfFile()
    {
        var path = AppContext.BaseDirectory;
        var fileName = _purpose.ToString().ToLowerInvariant();

        var config = new MqttConfig(
            ListenerPort: 8883,
            AllowAnonymous: false,
            CaFile: Path.Combine(path, CertificateAuthorityService.fileNameRootCA),
            CertFile: Path.Combine(path, $"{fileName}.crt"),
            KeyFile: Path.Combine(path, $"{fileName}.key"),
            RequireCertificate: true,
            UseIdentityAsUsername: true,
            TlsVersion: "tlsv1.3"
        );

        if (OperatingSystem.IsWindows())
        {
            File.AppendAllText(
                @"C:\Program Files\mosquitto\mosquitto.conf",
                ToMosquittoConfigString(config)
            );
        }
        else if (OperatingSystem.IsLinux())
        {
            File.WriteAllText("/etc/mosquitto/conf.d/local.conf", ToMosquittoConfigString(config));
        }
    }

    private string ToMosquittoConfigString(MqttConfig config)
    {
        return $"listener {config.ListenerPort}\n"
            + $"allow_anonymous {config.AllowAnonymous.ToString().ToLower()}\n"
            + $"cafile {config.CaFile}\n"
            + $"certfile {config.CertFile}\n"
            + $"keyfile {config.KeyFile}\n"
            + $"require_certificate {config.RequireCertificate.ToString().ToLower()}\n"
            + $"use_identity_as_username {config.UseIdentityAsUsername.ToString().ToLower()}\n"
            + $"tls_version {config.TlsVersion}\n";
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
