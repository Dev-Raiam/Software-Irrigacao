using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Security;
using Toolbox.Industrial.Core.Setup;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Data;

public delegate Task ApplicationSeedData(IServiceProvider serviceProvider, IEntityStore store);

public static class SeedData
{
    private static ILogger<IApplicationBuilder> _logger = null!;

    public static async Task EnsureSeedData(
        this IApplicationBuilder app,
        ApplicationSeedData? applicationSeedData = null
    )
    {
        using var scope = app
            .ApplicationServices.GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        try
        {
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<IApplicationBuilder>>();
            var store = scope.ServiceProvider.GetRequiredService<IEntityStore>();

            await InternalSeedData(scope.ServiceProvider, store);

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
    }

    private static async Task InternalSeedData(IServiceProvider provider, IEntityStore store)
    {
        //Manter a sequencia de execução porque a execução depende do processo anterior
        await ConfigureApiBaseAddress(store);
        await ConfigureJwtService(store, provider.GetRequiredService<JwtService>());
        await SynchronizeData(store, provider.GetRequiredService<IMediator>());
        await ConfigureMqttRemoto(store);
        await ConfigureMqttLocal(
            store,
            provider.GetRequiredService<Token>(),
            provider.GetRequiredService<ICertificateAuthorityService>()
        );
    }

    private static async Task ConfigureApiBaseAddress(IEntityStore store)
    {
        var id = Entity.Keys.Api.BaseAddress;
        var apiBaseAddress = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (apiBaseAddress?.Valor == null)
        {
            apiBaseAddress = new Configuracao(
                id: id,
                configuracao: "https://api.toolbox.app.br",
                grupo: Grupo.Api,
                tipo: Tipo.Config
            );
            await store.UpsertAsync(apiBaseAddress);
        }
        ApiClient.BaseAddress = apiBaseAddress.Valor.ToString();
    }

    private static async Task ConfigureJwtService(IEntityStore store, JwtService jwtService)
    {
        Guid id = Entity.Keys.Api.Jwt.ValidIssuers;
        var validIssuers = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (validIssuers?.Valor == null)
        {
            validIssuers = new Configuracao(
                id: id,
                configuracao: $"{ApiClient.BaseAddress}",
                grupo: Grupo.Auth,
                tipo: Tipo.Seguranca
            );
            await store.UpsertAsync(validIssuers);
        }
        JwtService.Config.ValidIssuers =
            validIssuers
                .Valor.ToString()
                ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        id = Entity.Keys.Api.Jwt.JwksUrl;
        var jwksUrl = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (jwksUrl?.Valor == null)
        {
            jwksUrl = new Configuracao(
                id: id,
                configuracao: $"{ApiClient.BaseAddress}/autenticacao/jwks",
                grupo: Grupo.Auth,
                tipo: Tipo.Seguranca
            );
            await store.UpsertAsync(jwksUrl);
        }
        JwtService.Config.JwksUrl = jwksUrl.Valor.ToString()!;

        await jwtService.LoadJwksAsync();
        if (!JwtService.Config.KeyStore.SigningKeys.Any())
        {
            var securityKeys = await store.FirstOrDefaultAsync<Configuracao>(x =>
                x.Id == Entity.Keys.Api.Jwt.SecKeys
            );
            if (securityKeys?.Valor != null)
            {
                await jwtService.LoadJwksAsync(securityKeys.Valor.ToString()!);
            }
        }
    }

    private static async Task SynchronizeData(IEntityStore store, IMediator mediator)
    {
        try
        {
            Guid.TryParse(
                (
                    await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.PainelId)
                )?.Valor.ToString(),
                out var painelId
            );
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

    private static async Task ConfigureMqttLocal(
        IEntityStore store,
        Token token,
        ICertificateAuthorityService authority
    )
    {
        Guid id = Entity.Keys.Mqtt.Local;
        var mqttLocal = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (mqttLocal?.Valor == null)
        {
            var config = new MqttConfiguration();
            mqttLocal = new Configuracao(
                id: id,
                configuracao: config,
                grupo: Grupo.Mqtt,
                tipo: Tipo.Config
            );
            await store.UpsertAsync(mqttLocal);
        }

        var controladores = store.Query<Controlador>().ToList();
        Guid.TryParse(
            (
                await store.FirstOrDefaultAsync<Configuracao>(x =>
                    x.Id == Entity.Keys.ControladorId
                )
            )?.Valor.ToString(),
            out var controladorId
        );
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
            var config = (MqttConfiguration)mqttLocal.Valor;
            var master = controladores.FirstOrDefault(c => c.Valor.Master);
            var masterHostName = master?.Valor.Conexoes.Host;
            if (master != null && masterHostName != null && config.Host != masterHostName)
            {
                var certificate = authority.GetCertificateStore(masterHostName)?.Valor as Certificate;
                if (certificate == null)
                {
                    //apagar as configurações e certificado de Mqtt Local
                    await store.DeleteAsync(mqttLocal);
                    await store.DeleteManyAsync<Configuracao>(x => x.Id == Entity.Keys.Security.CertificateMqttLocal);

                    await LoadCertificateAuthorityMaster(token, authority, masterHostName);

                    config.SetHost(masterHostName);
                    mqttLocal = new Configuracao(
                        id: id,
                        configuracao: config,
                        grupo: Grupo.Mqtt,
                        tipo: Tipo.Config
                    );
                    await store.UpsertAsync(mqttLocal);

                    await Task.Delay(1000);
                    Environment.Exit(1);
                    return;
                }

                config.SetHost(master.Valor.Conexoes.Host);
                mqttLocal = new Configuracao(
                    id: id,
                    configuracao: config,
                    grupo: Grupo.Mqtt,
                    tipo: Tipo.Config
                );
                await store.UpsertAsync(mqttLocal);
                await Task.Delay(1000);
                Environment.Exit(1);
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

    private static async Task LoadCertificateAuthorityMaster(
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
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var root = X509CertificateLoader.LoadCertificate(bytes);
                authority.Save(root, subject: masterHostName);
            }
            else
            {
                _logger.LogError(
                    "Falha ao obter certificado da autoridade certificadora do controlador master"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocorreu um erro ao obter certificado do controlador master: {ex}");
        }
    }

    private static async Task ConfigureMqttRemoto(IEntityStore store)
    {
        Guid id = Entity.Keys.Mqtt.Remoto;
        if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Valor == null)
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
