using System.Text;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
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
                // Application Discriminator (isolamento entre aplicações)
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
                    return new ApiClient(
                        httpClient,
                        provider.GetRequiredService<ILogger<ApiClient>>()
                    );
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
            WebApplicationBuilder builder,
            string connectionString
        )
        {
            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(connectionString));
            services.AddSingleton<IEntityStore, LiteDbEntityStore>();
            builder.Host.UseSerilog(
                async (context, provider, config) =>
                {
                    var store = provider.GetRequiredService<IEntityStore>();
                    var cfg = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Serilog.Config)
                        ?.Value.ToString();

                    if (cfg == null)
                    {
                        cfg = System.Text.Json.JsonSerializer.Serialize(
                            new SerilogConfig
                            {
                                Serilog = new SerilogConfig.serilog
                                {
                                    Using = new string[]
                                    {
                                        "Serilog.Sinks.Console",
                                        "Serilog.Sinks.LiteDB",
                                    },
                                    MinimumLevel = new MinimumLevelConfig
                                    {
                                        Default = "Information",
                                        Override = new Dictionary<string, string>
                                        {
                                            ["Microsoft"] = "Warning",
                                            ["System"] = "Warning",
                                        },
                                    },
                                    Enrich = new string[] { "FromLogContext", "WithMachineName" },
                                    WriteTo = new WriteToConfig[]
                                    {
                                        new WriteToConfig
                                        {
                                            Name = "LiteDB",
                                            Args = new Dictionary<string, object>
                                            {
                                                ["databaseUrl"] = connectionString,
                                                ["logCollectionName"] = "logs",
                                                ["restrictedToMinimumLevel"] = "Information",
                                            },
                                        },
                                    },
                                },
                            }
                        );

                        await store.UpsertAsync(
                            new Configuracao(id: Entity.Keys.Serilog.Config, value: cfg)
                        );
                    }
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(cfg));

                    var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
                    config.ReadFrom.Configuration(configuration);

                    //config = cfg.Serilog.MinimumLevel.Default switch
                    //{
                    //    LogEventLevel.Verbose => config.MinimumLevel.Verbose(),
                    //    LogEventLevel.Debug => config.MinimumLevel.Debug(),
                    //    LogEventLevel.Warning => config.MinimumLevel.Warning(),
                    //    LogEventLevel.Error => config.MinimumLevel.Error(),
                    //    LogEventLevel.Fatal => config.MinimumLevel.Fatal(),
                    //    _ => config.MinimumLevel.Information()
                    //};
                    //foreach (var item in cfg.Serilog.MinimumLevel.Override)
                    //{
                    //    config = config.MinimumLevel.Override(item.Key, item.Value);
                    //}
                    //foreach (var enrich in cfg.Serilog.Enrich)
                    //{
                    //    switch (enrich)
                    //    {
                    //        case "FromLogContext":
                    //            config = config.Enrich.FromLogContext();
                    //            break;
                    //        //case "WithMachineName":
                    //        //    config = config.Enrich.WithMachineName();
                    //        //    break;
                    //    }
                    //}

                    ////LogEventLevel? restricted = null;
                    ////var restrictedLevel = cfg.Serilog.WriteTo.FirstOrDefault(x => x.Name == "LiteDB")?.Args["restrictedToMinimumLevel"]?.ToString();
                    ////{
                    ////    restricted = restrictedLevel switch
                    ////    {
                    ////        "Verbose" => LogEventLevel.Verbose,
                    ////        "Debug" => LogEventLevel.Debug,
                    ////        "Information" => LogEventLevel.Information,
                    ////        "Warning" => LogEventLevel.Warning,
                    ////        "Error" => LogEventLevel.Error,
                    ////        "Fatal" => LogEventLevel.Fatal,
                    ////        _ => null
                    ////    };
                    ////};

                    //config = config.WriteTo.LiteDB(
                    //    databaseUrl: connectionString,
                    //    logCollectionName: "logs",
                    //    restrictedToMinimumLevel: restricted
                    //);
                }
            );
            return services;
        }
    }
}
