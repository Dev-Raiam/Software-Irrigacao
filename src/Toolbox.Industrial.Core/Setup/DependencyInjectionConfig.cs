using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Security.Cryptography;
using IMediator = Toolbox.Core.Mediator.IMediator;
using MediatorImp = Toolbox.Core.Mediator.Mediator;
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
                    ?.Valor.ToString();
            }

            if (ApiClient.BaseAddress != null)
            {
                http.BaseAddress = new Uri(ApiClient.BaseAddress);
            }
        }

        public static IServiceCollection AddIndustrialCore(
            this IServiceCollection services,
            params Assembly[] assemblies
        )
        {
            //services.AddMemoryCache();
            //services
            //    .AddJwksManager() // (options => options.Jws = Algorithm.Create(AlgorithmType.ECDsa, JwtType.Jws))
            //    .PersistKeysInMemory();

            //.PersistKeysToDatabaseStore<AutenticacaoDataContext>();
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

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;
            });

            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = { new StringEnumConverter() },
                };

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
                        ?.Valor;

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
                        ?.Valor;

                    return new MqttManager((MqttConfiguration?)config ?? new MqttConfiguration());
                }
            );

            #endregion Mqtt

            services.AddRateLimiter(options =>
            {
                options.AddConcurrencyLimiter(
                    Endpoints.RateLimitingPolicy,
                    options =>
                    {
                        options.PermitLimit = 2;
                        options.QueueLimit = 2;
                        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    }
                );
            });

            services.AddJwtConfiguration();
            //            typeof(SincronizarAutomacao).GetTypeInfo().Assembly
            services.AddSingleton<JwtService>();
            services.AddMediator([typeof(DependencyInjectionConfig).Assembly, .. assemblies]);
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
                        ?.Valor.ToString();

                    if (cfg == null)
                    {
                        cfg = System.Text.Json.JsonSerializer.Serialize(
                            new Configuration
                            {
                                Serilog = new Configuration.SerilogConfig
                                {
                                    Using = new string[]
                                    {
                                        "Serilog.Sinks.Console",
                                        "Serilog.Sinks.LiteDB",
                                    },
                                    MinimumLevel =
                                        new Configuration.SerilogConfig.MinimumLevelConfig
                                        {
                                            Default = "Information",
                                            Override = new Dictionary<string, string>
                                            {
                                                ["Microsoft"] = "Warning",
                                                ["System"] = "Warning",
                                            },
                                        },
                                    Enrich = new string[] { "FromLogContext", "WithMachineName" },
                                    WriteTo = new[]
                                    {
                                        new Configuration.SerilogConfig.WriteToConfig
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
                            new Configuracao(id: Entity.Keys.Serilog.Config, configuracao: cfg)
                        );
                    }

                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(cfg));
                    var configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
                    config.ReadFrom.Configuration(configuration);
                }
            );

            return services;
        }

        private static IServiceCollection AddMediator(
            this IServiceCollection services,
            params Assembly[] assemblies
        )
        {
            services.RegisterHandlers(
                assemblies,
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<>),
                typeof(IQueryHandler<,>),
                typeof(IEventHandler<>),
                typeof(IIntegrationHandler<>),
                typeof(INotificationHandler<>)
            );

            services.AddScoped<IMediator, MediatorImp>();
            return services;
        }

        private static void RegisterHandlers(
            this IServiceCollection services,
            Assembly[] assemblies,
            params Type[] handlerTypes
        )
        {
            var implementationTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .ToList();

            foreach (var implementationType in implementationTypes)
            {
                var interfaces = implementationType
                    .GetInterfaces()
                    .Where(i =>
                        handlerTypes.Any(handlerType =>
                            handlerType.IsGenericTypeDefinition
                                ? (i.IsGenericType && i.GetGenericTypeDefinition() == handlerType)
                                : handlerType.IsAssignableFrom(i)
                        )
                    );

                foreach (var @interface in interfaces)
                {
                    services.AddScoped(@interface, implementationType);
                }
            }
        }
    }

    internal class Configuration
    {
        public SerilogConfig Serilog { get; set; } = null!;

        public class SerilogConfig
        {
            public string[] Using { get; set; } = [];
            public string[] Enrich { get; set; } = [];
            public WriteToConfig[] WriteTo { get; set; } = [];
            public MinimumLevelConfig MinimumLevel { get; set; } = new();

            internal class MinimumLevelConfig
            {
                public string Default { get; set; } = "Information";
                public Dictionary<string, string> Override { get; set; } = new();
            }

            internal class WriteToConfig
            {
                public string Name { get; set; } = string.Empty;
                public Dictionary<string, object> Args { get; set; } = new();
            }
        }
    }
}
