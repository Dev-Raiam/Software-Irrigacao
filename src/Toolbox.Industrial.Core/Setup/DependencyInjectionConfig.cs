using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Communication.RaspIO;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Security;
using Toolbox.Industrial.Core.Security.Cryptography;
using Toolbox.Industrial.Core.Telemetry;
using Toolbox.Industrial.Core.Telemetry.Services;
using static Toolbox.Industrial.Core.Security.Certificate;
using Controlador = Toolbox.Industrial.Core.Data.Controlador;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using IMediator = Toolbox.Core.Mediator.IMediator;
using MediatorImp = Toolbox.Core.Mediator.Mediator;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Setup
{
    public static class DependencyInjectionConfig
    {
        private static void ConfigureClientAsync(IServiceProvider provider, HttpClient http)
        {
            if (ApiClient.BaseAddress == null)
            {
                var store = provider.GetRequiredService<IEntityStore>();
                ApiClient.BaseAddress = store
                    .ObterConfiguracao<string>(Entity.Keys.Api.BaseAddress)
                    .GetAwaiter()
                    .GetResult();
            }

            if (ApiClient.BaseAddress != null)
            {
                http.BaseAddress = new Uri(ApiClient.BaseAddress);
            }
        }

        private static void ConfigureHeartbeatClient(IServiceProvider provider, HttpClient client)
        {
            client.Timeout = TimeSpan.FromSeconds(5);
            if (
                ApiClient.BaseAddress != null
                && Controlador.PainelId != Guid.Empty
                && Controlador.ControladorId != Guid.Empty
            )
            {
                client.BaseAddress = new Uri(ApiClient.BaseAddress);
                var token = provider.GetRequiredService<Token>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token.TokenAcesso
                );
            }
        }

        // Metodo de Extensão para o Atualizador
        public static IServiceCollection AddLiteDbEntityStore(
            this IServiceCollection services,
            string connectionString
        )
        {
            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(connectionString));
            services.AddSingleton<IEntityStore, LiteDbEntityStore>();
            return services;
        }

        // Metodo de Extensão para o Atualizador
        public static IServiceCollection AddIndustrialCoreAtualizador(
            this IServiceCollection services
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
                    new DirectoryInfo("keys")
                );
            services
                .AddHttpClient<IApiClient, ApiClient>(ConfigureClientAsync)
                .AddHttpMessageHandler<AuthGuard>()
                .AddStandardResilienceHandler();

            services.AddKeyedTransient<IApiClient, ApiClient>(
                ApiClient.Anonymous,
                (provider, key) =>
                {
                    var httpClient = new HttpClient();
                    ConfigureClientAsync(provider, httpClient);
                    return new ApiClient(httpClient);
                }
            );

            return services;
        }

        public static IServiceCollection AddIndustrialCore(
            this IServiceCollection services,
            params Assembly[] assemblies
        )
        {
            services.AddSingleton<Token>();
            services.AddSingleton<EntityConfiguration>();
            services.AddSingleton<ICryptography, Cryptography>();
            services.AddSingleton<IControllerIO, PythonIoController>();
            services.TryAddSingleton<IPythonSettingsExporter, PythonSettingsExporter>();
            services.AddSingleton<ICertificateAuthorityService, CertificateAuthorityService>();
            services.AddSingleton<IConfigureOptions<KestrelServerOptions>, ConfigureKestrelHttps>();
            services.AddKeyedSingleton<ICertificateService>(
                Purpose.HttpsLocal,
                (provider, purpose) =>
                {
                    return new CertificateService(
                        (Purpose)purpose,
                        provider.GetRequiredService<IEntityStore>(),
                        provider.GetRequiredService<ILogger<CertificateService>>(),
                        provider.GetRequiredService<ICertificateAuthorityService>()
                    );
                }
            );

            services.AddKeyedSingleton<ICertificateService>(
                Purpose.MqttInterno,
                (provider, purpose) =>
                {
                    return new CertificateService(
                        (Purpose)purpose,
                        provider.GetRequiredService<IEntityStore>(),
                        provider.GetRequiredService<ILogger<CertificateService>>(),
                        provider.GetRequiredService<ICertificateAuthorityService>()
                    );
                }
            );

            services.AddKeyedSingleton<ICertificateService>(
                Purpose.MqttLocal,
                (provider, purpose) =>
                {
                    return new CertificateService(
                        (Purpose)purpose,
                        provider.GetRequiredService<IEntityStore>(),
                        provider.GetRequiredService<ILogger<CertificateService>>(),
                        provider.GetRequiredService<ICertificateAuthorityService>()
                    );
                }
            );

            services.AddKeyedSingleton<ICertificateService>(
                Purpose.MqttRemoto,
                (provider, purpose) =>
                {
                    return new CertificateService(
                        (Purpose)purpose,
                        provider.GetRequiredService<IEntityStore>(),
                        provider.GetRequiredService<ILogger<CertificateService>>(),
                        provider.GetRequiredService<ICertificateAuthorityService>()
                    );
                }
            );

            services.AddTransient<AuthGuard>();
            services
                .AddDataProtection()
                // Application Discriminator (isolamento entre aplicações)
                .SetApplicationName("Automacao")
                .PersistKeysToFileSystem(
                    new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "keys"))
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
                };

            services.AddSingleton<ISystemMetricsCollector, SystemMetricsCollector>();
            if (OperatingSystem.IsWindows())
            {
                services.AddSingleton<IMetricsProvider, WindowsMetricsProvider>();
            }
            if (OperatingSystem.IsLinux())
            {
                services.AddSingleton<IMetricsProvider, LinuxMetricsProvider>();
            }

            services.AddHostedService<SystemMetricsCollector>();
            services.AddHostedService<Heartbeat>();

            #region HttpClient
            services.AddHttpClient<IHeartbeatClient, HeartbeatClient>(ConfigureHeartbeatClient);

            services
                .AddHttpClient<IApiClient, ApiClient>(ConfigureClientAsync)
                .AddHttpMessageHandler<AuthGuard>()
                .AddStandardResilienceHandler();

            services.AddKeyedTransient<IApiClient, ApiClient>(
                ApiClient.Anonymous,
                (provider, key) =>
                {
                    var httpClient = new HttpClient();
                    ConfigureClientAsync(provider, httpClient);
                    return new ApiClient(httpClient);
                }
            );

            #endregion HttpClient

            #region Mqtt

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Interno,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current!
            );

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Local,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current!
            );

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Remoto,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current!
            );

            services.AddKeyedSingleton<MqttManager>(
                Mqtt.Interno,
                (provider, key) =>
                {
                    var store = provider.GetRequiredService<IEntityStore>();
                    var certificateService = provider.GetRequiredKeyedService<ICertificateService>(
                        Purpose.MqttInterno
                    );

                    var config = store
                        .ObterConfiguracao<MqttConfiguration>(Entity.Keys.Mqtt.Interno)
                        .GetAwaiter()
                        .GetResult();

                    var topics = new List<MqttTopicFilter>();
                    if (Controlador.ControladorId != Guid.Empty)
                    {
                        if (Controlador.Master)
                        {
                            //Saber se os Slaves estão comunicando.
                            topics.Add(
                                new MqttTopicFilterBuilder()
                                    .WithTopic($"heartbeats")
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                    .Build()
                            );

                            //receber resposta dos comandos (python/módulos)
                            topics.Add(
                                new MqttTopicFilterBuilder()
                                    .WithTopic(
                                        $"controladores/{Controlador.ControladorId}/comando/resposta"
                                    )
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                    .Build()
                            );

                            var controladores = store.Query<Controlador>().ToList();
                            foreach (var controlador in controladores)
                            {
                                //receber dados de telemetria interno (python/módulos) bem como telemetria dos slaves
                                //publicar no Remoto/LiteDB a telemetria interna.
                                topics.Add(
                                    new MqttTopicFilterBuilder()
                                        .WithTopic($"controladores/{controlador.Id}/telemetria")
                                        .WithQualityOfServiceLevel(
                                            MqttQualityOfServiceLevel.AtMostOnce
                                        )
                                        .Build()
                                );
                            }
                        }
                        else
                        {
                            //receber resposta dos comandos (python/módulos)
                            topics.Add(
                                new MqttTopicFilterBuilder()
                                    .WithTopic(
                                        $"controladores/{Controlador.ControladorId}/comando/resposta"
                                    )
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                    .Build()
                            );
                            //receber dados de telemetria (python/módulos) e publicar no Remoto/LiteDB
                            topics.Add(
                                new MqttTopicFilterBuilder()
                                    .WithTopic(
                                        $"controladores/{Controlador.ControladorId}/telemetria"
                                    )
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                    .Build()
                            );
                        }
                    }

                    var certificate = store.GetCertificate<Certificate>(
                        Entity.Keys.Security.CertificateMqttInterno
                    );
                    var subject = certificate?.Subject ?? config?.Host ?? "localhost";

                    var mqtt = new Mqtt(
                        provider: provider,
                        purpose: Mqtt.Interno,
                        topics: topics,
                        config: config ?? new MqttConfiguration(),
                        certificate: certificateService.GetCertificate(subject)
                    );

                    return new MqttManager(mqtt);
                }
            );

            services.AddKeyedSingleton<MqttManager>(
                Mqtt.Local,
                (provider, key) =>
                {
                    Mqtt? mqtt = null;

                    //Criar configuracao para permitir 2 master usar esse canal para se comunicar.
                    if (!Controlador.Master && Controlador.ControladorId != Guid.Empty)
                    {
                        var store = provider.GetRequiredService<IEntityStore>();
                        var certificateService =
                            provider.GetRequiredKeyedService<ICertificateService>(
                                Purpose.MqttLocal
                            );

                        var config = store
                            .ObterConfiguracao<MqttConfiguration>(Entity.Keys.Mqtt.Local)
                            .GetAwaiter()
                            .GetResult();

                        var topics = new List<MqttTopicFilter>();

                        //receber comandos do master ou internamente para execução.
                        topics.Add(
                            new MqttTopicFilterBuilder()
                                .WithTopic($"controladores/{Controlador.ControladorId}/comando")
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                .Build()
                        );

                        //receber resposta do python/módulo e publicar para o master quando necessário
                        topics.Add(
                            new MqttTopicFilterBuilder()
                                .WithTopic(
                                    $"controladores/{Controlador.ControladorId}/comando/resposta"
                                )
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                .Build()
                        );
                        //receber dados de telemetria interno (python/módulos) e publicar no Remoto/LiteDB,
                        //bem como publicar para o master quando necessário.
                        topics.Add(
                            new MqttTopicFilterBuilder()
                                .WithTopic($"controladores/{Controlador.ControladorId}/telemetria")
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                .Build()
                        );

                        var certificate = store.GetCertificate<Certificate>(
                            Entity.Keys.Security.CertificateMqttLocal
                        );

                        var subject = certificate?.Subject ?? config?.Host ?? "localhost";
                        mqtt = new Mqtt(
                            provider: provider,
                            purpose: Mqtt.Local,
                            topics: topics,
                            config: config ?? new MqttConfiguration(),
                            certificate: certificateService.GetCertificate(subject)
                        );
                    }

                    return new MqttManager(mqtt);
                }
            );

            services.AddKeyedSingleton<MqttManager>(
                Mqtt.Remoto,
                (provider, key) =>
                {
                    var store = provider.GetRequiredService<IEntityStore>();
                    var certificate = store.GetCertificate<Certificate>(
                        Entity.Keys.Security.CertificateMqttRemoto
                    );

                    var config = store
                        .ObterConfiguracao<MqttConfiguration>(Entity.Keys.Mqtt.Remoto)
                        .GetAwaiter()
                        .GetResult();

                    var topics = new List<MqttTopicFilter>();
                    //Somente o master pode inscrever em tópicos
                    if (Controlador.Master && Controlador.ControladorId != Guid.Empty)
                    {
                        var controladores = store.Query<Controlador>().ToList();
                        foreach (var controlador in controladores)
                        {
                            //Receber comandos externos (Master e Slaves),
                            //e publicar para os slaves os comandos destinados a eles
                            //após passar por validação se é possivel executar o comando.
                            topics.Add(
                                new MqttTopicFilterBuilder()
                                    .WithTopic($"controladores/{controlador.Id}/comando")
                                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                                    .Build()
                            );
                        }
                    }
                    config ??= new MqttConfiguration();
                    var subject = certificate?.Subject ?? "localhost";
                    X509Certificate2? certificado = null;
                    if (config.Port == 8883)
                    {
                        var certificateService =
                            provider.GetRequiredKeyedService<ICertificateService>(
                                Purpose.MqttRemoto
                            );
                        certificado = certificateService.GetCertificate(subject);
                    }

                    var mqtt = new Mqtt(
                        provider: provider,
                        purpose: Mqtt.Remoto,
                        topics: topics,
                        config: config,
                        certificate: certificado
                    );
                    return new MqttManager(mqtt);
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

        //private static void Register(this List<MqttTopicFilter> topics, Guid controladorId)
        //{

        //}

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
                    var cfg = await store.ObterConfiguracao<string>(Entity.Keys.Serilog.Config);

                    if (string.IsNullOrWhiteSpace(cfg))
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
                                            Name = "Console",
                                        },
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
                            new Configuracao(
                                id: Entity.Keys.Serilog.Config,
                                configuracao: cfg,
                                grupo: Grupo.Log,
                                tipo: Tipo.Config
                            )
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
