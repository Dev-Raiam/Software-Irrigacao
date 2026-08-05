using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Toolbox.Industrial.Core.Security.Certificate;

namespace Toolbox.Industrial.Core.Security;

internal sealed class ConfigureKestrelHttps : IConfigureOptions<KestrelServerOptions>
{
    private readonly ICertificateService _certificateService;

    public ConfigureKestrelHttps(
        [FromKeyedServices(Purpose.HttpsLocal)] ICertificateService certificateService
    )
    {
        _certificateService = certificateService;
    }

    public void Configure(KestrelServerOptions options)
    {
        options.ListenAnyIP(80);
        options.ListenAnyIP(
            443,
            listen =>
            {
                listen.UseHttps(_certificateService.GetCertificate());
            }
        );
    }
}
