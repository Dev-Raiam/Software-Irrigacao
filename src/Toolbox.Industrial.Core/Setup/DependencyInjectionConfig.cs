using LiteDB;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Security.Cryptography;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;

namespace Toolbox.Industrial.Core.Setup
{
    public static class DependencyInjectionConfig
    {
        private static void ConfigureClient(IServiceProvider provider, HttpClient http)
        {
            if (ApiClient.BaseAddress == null)
            {
                var store = provider.GetRequiredService<IEntityStore>();
                ApiClient.BaseAddress = store
                    .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Api.BaseAddress)
                    ?.Value.ToString();
            }

            if (ApiClient.BaseAddress != null)
            {
                http.BaseAddress = new Uri(ApiClient.BaseAddress);
            }
        }


        public static IServiceCollection AddIndustrialCore(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddSingleton<Token>();
            services.AddSingleton<EntityConfiguration>();
            services.AddSingleton<ICryptography, Cryptography>();
            services.AddTransient<AuthGuard>();
            services
                .AddDataProtection()
                .SetApplicationName("Automacao")
                .PersistKeysToFileSystem(
                    new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Keys"))
                );

            #region HttpClient

            services
                .AddHttpClient<IApiClient, ApiClient>(ConfigureClient)
                .AddHttpMessageHandler<AuthGuard>()
                .AddStandardResilienceHandler();

            services.AddKeyedTransient<IApiClient, ApiClient>(
                ApiClient.Anonymous,
                (provider, key) =>
                {
                    var httpClient = new HttpClient();
                    ConfigureClient(provider, httpClient);
                    return new ApiClient(httpClient, provider.GetRequiredService<ILogger<ApiClient>>());
                }
            );

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
                    var config = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Mqtt.Local)
                        ?.Value;

                    return new MqttManager((MqttConfiguration?)config ?? new MqttConfiguration());
                }
            );

            services.AddKeyedSingleton<MqttManager>(
                Mqtt.Remoto,
                (provider, key) =>
                {
                    var store = provider.GetRequiredService<IEntityStore>();
                    var config = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Mqtt.Remoto)
                        ?.Value;

                    return new MqttManager((MqttConfiguration?)config ?? new MqttConfiguration());
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
