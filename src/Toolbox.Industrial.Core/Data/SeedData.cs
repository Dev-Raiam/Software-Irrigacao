using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Setup;
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

        var store = scope.ServiceProvider.GetRequiredService<IEntityStore>();
        var exporter = scope.ServiceProvider.GetRequiredService<IPythonSettingsExporter>();

        await InternalSeedData(scope.ServiceProvider, store);

        if (applicationSeedData != null)
        {
            await applicationSeedData(scope.ServiceProvider, store);
        }
        if (!exporter.Exported)
        {
            await exporter.ExportAsync();
        }
    }

    private static async Task InternalSeedData(IServiceProvider provider, IEntityStore store)
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

        id = Entity.Keys.Api.Jwt.ValidIssuers;
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

        var jwtService = provider.GetRequiredService<JwtService>();
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

        Controlador.Master = false;
        var controladores = store.Query<Controlador>().ToList();
        Guid.TryParse(
            (
                await store.FirstOrDefaultAsync<Configuracao>(x =>
                    x.Id == Entity.Keys.ControladorId
                )
            )?.Valor.ToString(),
            out var controladorId
        );

        if (controladorId != Guid.Empty)
        {
            Controlador.Master =
                controladores.FirstOrDefault(c => c.Id == controladorId)?.Valor.Master ?? false;
        }
        else if (controladores.Count == 1)
        {
            Controlador.Master = controladores.First().Valor.Master;
        }

        id = Entity.Keys.Mqtt.Local;
        var mqttLocal = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (mqttLocal?.Valor == null)
        {
            var config = new MqttConfiguration { Username = "master", Password = "broker@MQ" };
            mqttLocal = new Configuracao(
                id: id,
                configuracao: config,
                grupo: Grupo.Mqtt,
                tipo: Tipo.Config
            );
            await store.UpsertAsync(mqttLocal);
        }
        if (!Controlador.Master)
        {
            var config = (MqttConfiguration)mqttLocal.Valor;
            var master = controladores.FirstOrDefault(c => c.Valor.Master);
            if (master != null && config.Host != master.Valor.Conexoes.Host)
            {
                config.SetHost(master.Valor.Conexoes.Host);
                mqttLocal = new Configuracao(
                    id: id,
                    configuracao: config,
                    grupo: Grupo.Mqtt,
                    tipo: Tipo.Config
                );
                await store.UpsertAsync(mqttLocal);
            }
        }

        id = Entity.Keys.Mqtt.LocalPython;
        if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Valor == null)
        {
            var config = new MqttConfiguration { Username = "master", Password = "broker@MQ" };

            await store.UpsertAsync(
                new Configuracao(id: id, configuracao: config, grupo: Grupo.Mqtt, tipo: Tipo.Config)
            );
        }

        id = Entity.Keys.Mqtt.Remoto;
        if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Valor == null)
        {
            var config = new MqttConfiguration(host: "broker.freemqtt.com")
            {
                Username = "freemqtt",
                Password = "public",
            };

            await store.UpsertAsync(
                new Configuracao(id: id, configuracao: config, grupo: Grupo.Mqtt, tipo: Tipo.Config)
            );
        }
    }
}
