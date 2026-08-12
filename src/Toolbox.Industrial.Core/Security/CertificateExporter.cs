using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Toolbox.Industrial.Core.Security;

internal static class CertificateExporter
{
    public static void Export(X509Certificate2 certificate, string fileName)
    {
        ExportCertificate(certificate, $"{fileName}.crt");
        ExportPrivateKey(certificate, $"{fileName}.key");
    }

    public static void ExportCertificate(X509Certificate2 certificate, string path)
    {
        var pem = PemEncoding.Write("CERTIFICATE", certificate.Export(X509ContentType.Cert));

        File.WriteAllText(path, pem);
    }

    public static void ExportPrivateKey(
        X509Certificate2 certificate,
        string path)
    {
        if (!certificate.HasPrivateKey)
        {
            throw new CryptographicException(
                "O certificado não possui chave privada.");
        }
        if (certificate.GetECDsaPrivateKey() is ECDsa ecdsa)
        {
            var pem = PemEncoding.Write(
                "PRIVATE KEY",
                ecdsa.ExportPkcs8PrivateKey());

            File.WriteAllText(path, pem);

            return;
        }

        if (certificate.GetRSAPrivateKey() is RSA rsa)
        {
            var pem = PemEncoding.Write(
                "PRIVATE KEY",
                rsa.ExportPkcs8PrivateKey());

            File.WriteAllText(path, pem);

            return;
        }

        throw new NotSupportedException(
            "Unsupported private key algorithm.");
    }

    //public static void ExportPrivateKey(X509Certificate2 certificate, string path)
    //{
    //    char[] pem;

    //    if (certificate.GetECDsaPrivateKey() is ECDsa ecdsa)
    //    {
    //        pem = PemEncoding.Write("PRIVATE KEY", ecdsa.ExportPkcs8PrivateKey());
    //    }
    //    else if (certificate.GetRSAPrivateKey() is RSA rsa)
    //    {
    //        pem = PemEncoding.Write("PRIVATE KEY", rsa.ExportPkcs8PrivateKey());
    //    }
    //    else
    //    {
    //        throw new NotSupportedException("Unsupported private key algorithm.");
    //    }

    //    File.WriteAllText(path, pem);
    //}
}
