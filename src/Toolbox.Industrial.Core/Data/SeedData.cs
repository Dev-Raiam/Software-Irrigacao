using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Industrial.Core.Communication.Api;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;

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

        await InternalSeedData(store);

        if (applicationSeedData != null)
        {
            await applicationSeedData(scope.ServiceProvider, store);
        }
    }

    private static async Task InternalSeedData(IEntityStore store)
    {
        var id = Entity.Keys.Api.BaseAddress;
        var apiBaseAddress = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (apiBaseAddress?.Valor == null)
        {
            apiBaseAddress = new Configuracao(id: id, configuracao: "https://api.toolbox.app.br");
            await store.UpsertAsync(apiBaseAddress);
        }
        ApiClient.BaseAddress = apiBaseAddress.Valor.ToString();

        id = Entity.Keys.Api.Jwt.Issuers;
        var jwtIssuers = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (jwtIssuers?.Valor == null)
        {
            jwtIssuers = new Configuracao(id: id, configuracao: $"{ApiClient.BaseAddress}");
            await store.UpsertAsync(jwtIssuers);
        }
        ApiClient.JwtIssuers = jwtIssuers.Valor.ToString();

        id = Entity.Keys.Api.Jwt.JwksUrl;
        var jwtJwksUrl = await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id);
        if (jwtJwksUrl?.Valor == null)
        {
            jwtJwksUrl = new Configuracao(id: id, configuracao: $"{ApiClient.BaseAddress}/autenticacao/jwks");
            await store.UpsertAsync(jwtJwksUrl);
        }
        ApiClient.JwtJwksUrl = jwtJwksUrl.Valor.ToString();

        id = Entity.Keys.Mqtt.Local;
        if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Valor == null)
        {
            var config = new MqttConfiguration { Username = "master", Password = "broker@MQ" };

            await store.UpsertAsync(
                new Configuracao(
                    id: id,
                    configuracao: config
                )
            );
        }

        id = Entity.Keys.Mqtt.Remoto;
        if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Valor == null)
        {
            var config = new MqttConfiguration
            {
                Host = "broker.freemqtt.com",
                Username = "freemqtt",
                Password = "public",
            };

            await store.UpsertAsync(new Configuracao(id: id, configuracao: config));
        }
    }
}
