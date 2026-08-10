using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Core.Extensions;
using static Toolbox.Industrial.Core.Security.Certificate;

namespace Toolbox.Industrial.Core.Security;

internal static class CertificateFactory
{
    public static X509Certificate2 Create(
        Purpose purpose,
        ICertificateAuthorityService authorityService,
        string subject = "localhost"
    )
    {
        subject.ThrowIfNull(nameof(subject));

        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var request = new CertificateRequest(
            $"CN=Toolbox.Industrial.Core",
            ecdsa,
            HashAlgorithmName.SHA256
        );

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false)
        );

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                false
            )
        );

        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false)
        );

        var eku = new OidCollection();
        switch (purpose)
        {
            case Purpose.HttpsLocal:
                eku.Add(new Oid("1.3.6.1.5.5.7.3.1")); //Server Authentication
                break;

            case Purpose.MqttLocal:
                eku.Add(new Oid("1.3.6.1.5.5.7.3.1")); //Server Authentication
                eku.Add(new Oid("1.3.6.1.5.5.7.3.2")); //Client Authentication
                break;
        }

        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(eku, critical: true));

        request.CertificateExtensions.Add(BuildSubjectAlternativeNames(subject));

        var certificate = authorityService.Sign(
            request,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(20),
            subject
        );

        certificate = certificate.CopyWithPrivateKey(ecdsa);

        if (OperatingSystem.IsWindows())
        {
            certificate.FriendlyName = $"Toolbox Industrial {purpose}";
        }

        return certificate;
    }

    private static X509Extension BuildSubjectAlternativeNames(string subject)
    {
        var san = new SubjectAlternativeNameBuilder();

        if (!subject.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
            !subject.Equals(CertificateService.Kestrel, StringComparison.OrdinalIgnoreCase))
        {
            san.AddDnsName(subject);
        }

        san.AddDnsName("localhost");
        san.AddDnsName(Environment.MachineName);

        try
        {
            var host = Dns.GetHostEntry(Environment.MachineName);

            if (
                !string.IsNullOrWhiteSpace(host.HostName)
                && host.HostName != Environment.MachineName
            )
                san.AddDnsName(host.HostName);
        }
        catch { }

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
                continue;

            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            if (!Enum.IsDefined(nic.NetworkInterfaceType))
                continue;

            foreach (var ip in nic.GetIPProperties().UnicastAddresses)
            {
                san.AddIpAddress(ip.Address);
            }
        }

        return san.Build();
    }
}
