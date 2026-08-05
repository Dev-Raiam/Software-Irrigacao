using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Security;
using Toolbox.Industrial.Core.Setup;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Data;

public delegate Task ApplicationSeedData(IServiceProvider serviceProvider, IEntityStore store);

public static class SeedData
{
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
            Log.Error(ex, "Erro ao inicializar dados");
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
        }
        catch { }
    }

    private static async Task ConfigureMqttLocal(
        IEntityStore store,
        Token token,
        ICertificateAuthorityService authorityService
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
        Controlador.Master = true;
        Controlador.ControladorId = controladorId;
        if (controladorId != Guid.Empty)
        {
            Controlador.Master =
                controladores.FirstOrDefault(c => c.Id == controladorId)?.Valor.Master ?? false;
        }
        else if (controladores.Count == 1)
        {
            var controlador = controladores.First().Valor;
            Controlador.Master = controlador.Master;
            Controlador.ControladorId = controlador.Id;
        }
        else if (controladores.Count > 1)
        {
            Log.Error("Configure um controlador para o processo.");
        }

        if (!Controlador.Master)
        {
            var config = (MqttConfiguration)mqttLocal.Valor;
            var master = controladores.FirstOrDefault(c => c.Valor.Master);
            var masterHostName = master?.Valor.Conexoes.Host;
            if (master != null && config.Host != masterHostName)
            {
                var data =
                    store
                        .FirstOrDefault<Configuracao>(x =>
                            x.Id == Entity.Keys.Security.CertificateAuthority
                        )
                        ?.Valor as Certificate;

                if (
                    data != null
                    && data.Subject != masterHostName!.ToLowerInvariant().GetId().ToString()
                )
                {
                    await store.DeleteAsync(mqttLocal);
                    await LoadCertificateAuthorityMaster(token, authorityService, masterHostName);
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

                Environment.Exit(1);
                return;
            }
        }
    }

    private static async Task LoadCertificateAuthorityMaster(
        Token token,
        ICertificateAuthorityService authorityService,
        string masterHostName
    )
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
                authorityService.Save(
                    X509CertificateLoader.LoadPkcs12(
                        data.Content,
                        data.Password,
                        X509KeyStorageFlags.Exportable
                    ),
                    subject: masterHostName.ToLowerInvariant().GetId().ToString()
                );
            }
            //Certificate
            //Security.CertificateAuthority = response.Data.Id;
        }
        else
        {
            Log.Error(
                "Falha ao obter certificado da autoridade certificadora do controlador master"
            );
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
