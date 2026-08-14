using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Security;
using Controlador = Toolbox.Industrial.Core.Data.Controlador;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Setup;

public delegate Task ApplicationSeedData(IServiceProvider serviceProvider, IEntityStore store);

public static class Application
{
    public static Guid IntegracaoId => _integracaoId;

    public static Stopwatch Stopwatch = Stopwatch.StartNew();
    private static ILogger<IApplicationBuilder> _logger = null!;
    private static Guid _integracaoId = Guid.Empty;
    private static IHost _app = null!;

    public static void UpdateRun(HostApplicationBuilder builder, Guid integracaoId, string connectionString)
    {
        _integracaoId = integracaoId;
        builder.Services.AddIndustrialCoreAtualizador().AddLiteDbEntityStore(connectionString);
        _app = builder.Build();
        _app.Run();
    }

    public static async Task RunAsync(
        WebApplication app,
        Guid integracaoId,
        ApplicationSeedData? applicationSeedData = null
    )
    {
        _integracaoId = integracaoId;

        _app = app;
        try
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            _logger = scope.ServiceProvider.GetRequiredService<ILogger<IApplicationBuilder>>();
            var store = scope.ServiceProvider.GetRequiredService<IEntityStore>();

            await EnsureSeedData(scope.ServiceProvider, store);

            var exporter = scope.ServiceProvider.GetRequiredService<IPythonSettingsExporter>();
            if (applicationSeedData != null)
            {
                await applicationSeedData.Invoke(scope.ServiceProvider, store);
            }
            if (!exporter.Exported)
            {
                await exporter.ExportAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar dados");
        }
        Stopwatch.Stop();
        Log.Information("Aplicação inicializada. Tempo Decorrido({Elapsed})", Stopwatch.Elapsed);
        await app.RunAsync();
    }

    public static async Task Restart()
    {
        //"architecture":"aarch64"
        // var architecture = RuntimeInformation.OSArchitecture.ToString();
        //"operatingSystem":"Debian GNU/Linux 12 (bookworm)"
        var operatingSystem = RuntimeInformation.OSDescription;
        if (
            operatingSystem.Contains("Debian", StringComparison.OrdinalIgnoreCase)
            || operatingSystem.Contains("Windows", StringComparison.OrdinalIgnoreCase)
        )
        {
            await Task.Delay(1000);
            Environment.Exit(1);
            return;
        }
        var lifetime = _app.Services.GetRequiredService<IHostApplicationLifetime>();
        await Task.Delay(1000);
        //Environment.FailFast(("");
        lifetime.StopApplication();

    }

    private static async Task EnsureSeedData(IServiceProvider provider, IEntityStore store)
    {
        //Manter a sequencia de execução porque a execução depende do processo anterior
        await ConfigureApiBaseAddress(store);
        await ConfigureJwtService(store, provider.GetRequiredService<JwtService>());
        await SynchronizeData(store, provider.GetRequiredService<IMediator>());
        await ConfigureMqttRemoto(store);
        await ConfigureMqtt(
            store,
            provider.GetRequiredService<Token>(),
            provider.GetRequiredService<ICertificateAuthorityService>()
        );
    }

    private static async Task ConfigureApiBaseAddress(IEntityStore store)
    {
        var id = Entity.Keys.Api.BaseAddress;
        var apiBaseAddress = await store.ObterConfiguracao<string>(id);
        if (string.IsNullOrWhiteSpace(apiBaseAddress))
        {
            apiBaseAddress = "https://api.toolbox.app.br";
            await store.UpsertAsync(
                new Configuracao(
                    id: id,
                    configuracao: apiBaseAddress,
                    grupo: Grupo.Api,
                    tipo: Tipo.Config
                )
            );
        }
        ApiClient.BaseAddress = apiBaseAddress;
    }

    private static async Task ConfigureJwtService(IEntityStore store, JwtService jwtService)
    {
        var id = Entity.Keys.Api.Jwt.ValidIssuers;
        var validIssuers = await store.ObterConfiguracao<string>(id);
        if (string.IsNullOrWhiteSpace(validIssuers))
        {
            validIssuers = $"{ApiClient.BaseAddress}";
            await store.UpsertAsync(
                new Configuracao(
                    id: id,
                    configuracao: validIssuers,
                    grupo: Grupo.Auth,
                    tipo: Tipo.Seguranca
                )
            );
        }
        JwtService.Config.ValidIssuers =
            validIssuers?.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ) ?? [];

        id = Entity.Keys.Api.Jwt.JwksUrl;
        var jwksUrl = await store.ObterConfiguracao<string>(id);
        if (string.IsNullOrWhiteSpace(jwksUrl))
        {
            jwksUrl = $"{ApiClient.BaseAddress}/autenticacao/jwks";
            await store.UpsertAsync(
                new Configuracao(
                    id: id,
                    configuracao: jwksUrl,
                    grupo: Grupo.Auth,
                    tipo: Tipo.Seguranca
                )
            );
        }
        JwtService.Config.JwksUrl = jwksUrl!;

        await jwtService.LoadJwksAsync();
        if (!JwtService.Config.KeyStore.SigningKeys.Any())
        {
            var securityKeys = await store.ObterConfiguracao<string>(Entity.Keys.Api.Jwt.SecKeys);
            if (!string.IsNullOrWhiteSpace(securityKeys))
            {
                await jwtService.LoadJwksAsync(securityKeys!);
            }
        }
    }

    private static async Task SynchronizeData(IEntityStore store, IMediator mediator)
    {
        try
        {
            var painelId = await store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
            Controlador.PainelId = painelId;
            if (painelId != Guid.Empty)
            {
                await mediator.Execute(
                    new SincronizarAutomacao { PainelId = painelId },
                    cancellationToken: default
                );
            }
            var existeTelemetrias = store.Query<Telemetria>().FirstOrDefault() != null;
        }
        catch { }
    }

    private static async Task ConfigureMqtt(
        IEntityStore store,
        Token token,
        ICertificateAuthorityService authority
    )
    {
        Configuracao? mqttLocal = null;
        var mqttInterno = await store.ObterConfiguracao<Configuracao>(Entity.Keys.Mqtt.Interno);
        if (mqttInterno?.Valor == null)
        {
            var config = new MqttConfiguration();
            mqttInterno = new Configuracao(
                id: Entity.Keys.Mqtt.Interno,
                configuracao: config,
                grupo: Grupo.Mqtt,
                tipo: Tipo.Config
            );
            await store.UpsertAsync(mqttInterno);
        }

        var controladores = store.Query<Controlador>().ToList();

        var controladorId = await store.ObterConfiguracao<Guid>(Entity.Keys.ControladorId);
        //Controlador.Master = false;
        Controlador.ControladorId = controladorId;
        if (controladorId != Guid.Empty)
        {
            var controlador = controladores.FirstOrDefault(c => c.Id == controladorId)?.Valor;
            Controlador.Master = controlador?.Master ?? false;

            if (controlador != null)
            {
                await SetHostName(controlador.Conexoes.Host);
            }
        }
        else if (controladores.Count == 1)
        {
            var controlador = controladores.First().Valor;
            Controlador.Master = controlador.Master;
            Controlador.ControladorId = controlador.Id;

            await SetHostName(controlador.Conexoes.Host);
        }
        else if (controladores.Count > 1)
        {
            _logger.LogError("Configure um controlador para o processo.");
        }

        if (controladores.Count > 0 && !Controlador.Master)
        {
            mqttLocal = await store.ObterConfiguracao<Configuracao>(Entity.Keys.Mqtt.Local);
            if (mqttLocal?.Valor == null)
            {
                mqttLocal = new Configuracao(
                    id: Entity.Keys.Mqtt.Local,
                    configuracao: new MqttConfiguration(),
                    grupo: Grupo.Mqtt,
                    tipo: Tipo.Config
                );
                await store.UpsertAsync(mqttLocal);
            }

            var config = (MqttConfiguration)mqttLocal.Valor;
            var master = controladores.FirstOrDefault(c => c.Valor.Master);
            var masterHostName = master?.Valor.Conexoes.Host;
            if (master != null && masterHostName != null && config.Host != masterHostName)
            {
                var certificate = store.GetCertificate<Certificate>(
                    Entity.Keys.Security.CertificateAuthority,
                    masterHostName
                );
                if (certificate == null)
                {
                    await store.DeleteManyAsync<Configuracao>(x =>
                        x.Id == Entity.Keys.Security.CertificateMqttLocal
                    );
                    await LoadCertificateAuthorityMaster(token, authority, masterHostName);

                    config.SetHost(masterHostName);
                    //mqttLocal = new Configuracao(
                    //    id: id,
                    //    configuracao: config,
                    //    grupo: Grupo.Mqtt,
                    //    tipo: Tipo.Config
                    //);
                    await store.UpsertAsync(mqttLocal);
                    _logger.LogWarning(
                        $"A aplicação será finalizada para completar a implantação do certificado do {masterHostName}"
                    );
                    await Application.Restart();
                    return;
                }

                config.SetHost(master.Valor.Conexoes.Host);
                mqttLocal = new Configuracao(
                    id: mqttLocal.Id,
                    configuracao: config,
                    grupo: Grupo.Mqtt,
                    tipo: Tipo.Config
                );
                await store.UpsertAsync(mqttLocal);
                _logger.LogWarning(
                    $"A aplicação será finalizada para completar a configuração do certificado do {masterHostName}"
                );
                await Application.Restart();
                return;
            }
        }
    }

    private static async Task SetHostName(string hostName)
    {
        var host = Dns.GetHostEntry(Environment.MachineName);

        if (
            !string.IsNullOrEmpty(hostName)
            && !host.HostName.Equals(hostName, StringComparison.OrdinalIgnoreCase)
        )
        {
            if (OperatingSystem.IsLinux())
            {
                using var process = new Process();

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "sudo",
                    ArgumentList = { "hostnamectl", "set-hostname", hostName },
                };

                process.Start();
                await process.WaitForExitAsync();

                var exitCode = process.ExitCode;
                if (exitCode == 0)
                {
                    UpdateEtcHosts(hostName);
                    await Reboot();
                }
            }
            //if (OperatingSystem.IsWindows())
            //{
            //    process.StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "powershell",
            //        ArgumentList =
            //        {
            //            "-Command",
            //            $"Rename-Computer -NewName \"{hostName}\" -Force"
            //        }
            //    };
            //}
        }
    }

    private static async Task Reboot()
    {
        if (OperatingSystem.IsLinux())
        {
            using var process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "systemctl",
                ArgumentList = { "reboot" },
            };

            process.Start();
            await process.WaitForExitAsync();
        }
        //if (OperatingSystem.IsWindows())
        //{
        //    process.StartInfo = new ProcessStartInfo
        //    {
        //        FileName = "Restart-Computer",
        //        ArgumentList =
        //        {
        //            "/r", // Restart
        //            "/f", //Força o encerramento dos aplicativos.
        //            "/t",
        //            "0", // Sem atraso <segundos>
        //        },
        //        UseShellExecute = false,
        //    };
        //}
    }

    private static void UpdateEtcHosts(string hostName)
    {
        var path = "/etc/hosts";
        var lines = File.ReadAllLines(path).ToList();

        bool found = false;

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("127.0.1.1"))
            {
                lines[i] = $"127.0.1.1\t{hostName}";
                found = true;
                break;
            }
        }

        if (!found)
        {
            lines.Add($"127.0.1.1\t{hostName}");
        }

        File.WriteAllLines(path, lines);
    }

    internal static async Task LoadCertificateAuthorityMaster(
        Token token,
        ICertificateAuthorityService authority,
        string masterHostName
    )
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            httpClient.BaseAddress = new Uri($"http://{masterHostName}");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.TokenAcesso
            );
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"system/security/certificate-authority/{Entity.Keys.Security.CertificateAuthority}"
            );

            using var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<Certificate>();
                if (data != null)
                {
                    authority.Save(
                        X509CertificateLoader.LoadPkcs12(
                            data.Content,
                            data.Password,
                            X509KeyStorageFlags.Exportable
                        ),
                        subject: masterHostName
                    );
                }
            }
            else
            {
                _logger.LogError(
                    "Falha ao obter certificado da autoridade certificadora do controlador master"
                );
                await Application.Restart();
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocorreu um erro ao obter certificado do controlador master: {ex}");
            await Application.Restart();
            return;
        }
    }

    private static async Task ConfigureMqttRemoto(IEntityStore store)
    {
        Guid id = Entity.Keys.Mqtt.Remoto;
        if ((await store.ObterConfiguracao<Configuracao>(id)) == null)
        {
            var config = new MqttConfiguration(host: "broker.freemqtt.com")
            {
                Username = "freemqtt",
                Password = "public",
                Port = 1883,
            };

            await store.UpsertAsync(
                new Configuracao(id: id, configuracao: config, grupo: Grupo.Mqtt, tipo: Tipo.Config)
            );
        }
    }
}
