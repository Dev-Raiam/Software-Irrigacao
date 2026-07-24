using LiteDB;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Security.Cryptography;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;

namespace Toolbox.Industrial.Core.Setup
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddIndustrialCore(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddSingleton<Token>();
            services.AddSingleton<EntityConfiguration>();
            services.AddTransient<AuthGuard>();
            services
                .AddDataProtection()
                .SetApplicationName("Automacao")
                .PersistKeysToFileSystem(
                    new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Keys"))
                );

            services.AddSingleton<ICryptography, Cryptography>();

            #region HttpClient

            services
                .AddHttpClient<IApiClient, ApiClient>(
                    (provider, http) =>
                    {
                        var store = provider.GetRequiredService<IEntityStore>();
                        var uri = store
                            .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Api.BaseAddress)
                            ?.Value;
                        if (uri != null)
                        {
                            http.BaseAddress = new Uri(uri);
                        }
                    }
                )
                .AddStandardResilienceHandler();

            services
                .AddHttpClient(
                    HttpClientNames.Automacao,
                    (provider, http) =>
                    {
                        var store = provider.GetRequiredService<IEntityStore>();
                        var uri = store
                            .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Api.BaseAddress)
                            ?.Value;

                        if (uri != null)
                        {
                            http.BaseAddress = new Uri(uri);
                        }
                    }
                )
                .AddHttpMessageHandler<AuthGuard>()
                .AddStandardResilienceHandler();

            // services.AddTransient<IApiClient, ApiClient>();

            #endregion HttpClient

            #region Mqtt

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Local,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current
            );

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Remoto,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current
            );

            services.AddKeyedSingleton<MqttManager>(
                Mqtt.Local,
                (provider, key) =>
                {
                    var store = provider.GetRequiredService<IEntityStore>();
                    var json = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Mqtt.Local)
                        ?.Value;

                    var config =
                        json != null ? JsonSerializer.Deserialize<MqttConfiguration>(json) : null;

                    return new MqttManager(config ?? new MqttConfiguration());
                }
            );

            services.AddKeyedSingleton<MqttManager>(
                Mqtt.Remoto,
                (provider, key) =>
                {
                    //var config = provider.GetRequiredKeyedService<IOptions<Configuration>>(key).Value;
                    //var config = new MqttConfiguration
                    //{
                    //    Host = "broker.freemqtt.com",
                    //    Port = 1883,
                    //    ClientId = Guid.NewGuid().ToString(),
                    //    Username = "freemqtt",
                    //    Password = "public",
                    //};
                    var store = provider.GetRequiredService<IEntityStore>();
                    ///TODO: await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.Mqtt.Local);
                    var json = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Mqtt.Remoto)
                        ?.Value;
                    var config =
                        json != null ? JsonSerializer.Deserialize<MqttConfiguration>(json) : null;
                    return new MqttManager(config ?? new MqttConfiguration());
                }
            );

            #endregion Mqtt

            return services;
        }

        public static IServiceCollection AddLiteDbEntityStore(
            this IServiceCollection services,
            string connectionString
        )
        {
            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(connectionString));
            services.AddSingleton<IEntityStore, LiteDbEntityStore>();
            return services;
        }
    }
}
