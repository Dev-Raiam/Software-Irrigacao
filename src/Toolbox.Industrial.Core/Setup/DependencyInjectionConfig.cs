using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services.Api;
using Toolbox.Automacao.Core.Services.Cryptography;
using Toolbox.Automacao.Core.Services.Mqtt;

namespace Toolbox.Automacao.Core.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            //services.AddScoped<IConfiguracaoAutenticacao, ConfiguracaoAutenticacao>();

            services.AddTransient<IRepository, Repository>();

            services.AddTransient<IApiClient, ApiClient>();

            //services.AddTransient<ISincronizacao, Sincronizacao>();

            //services.AddTransient<IDadosSincronizacao, DadosSincronizacao>();

            services.AddTransient<ICryptography, Cryptography>();

            services.AddTransient<AuthGuard>();

            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(
                @"Filename=Automacao.db;Connection=Shared"
            ));

            services.AddSingleton<EntityConfiguration>();

            services.AddKeyedSingleton(
                Mqtt.Local,
                (provider, key) =>
                {
                    var config = provider
                        .GetRequiredKeyedService<IOptions<Services.Mqtt.Configuration>>(key)
                        .Value;
                    return new MqttManager(config);
                }
            );

            services.AddKeyedSingleton(
                Mqtt.Remoto,
                (provider, key) =>
                {
                    //var config = provider.GetRequiredKeyedService<IOptions<Configuration>>(key).Value;
                    var config = new Services.Mqtt.Configuration
                    {
                        Host = "broker.freemqtt.com",
                        Port = 1883,
                        ClientId = Guid.NewGuid().ToString(),
                        Username = "freemqtt",
                        Password = "public",
                    };
                    return new MqttManager(config);
                }
            );

            services.AddSingleton<Token>();

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Local,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current
            );

            services.AddKeyedTransient<IMqtt>(
                Mqtt.Remoto,
                (provider, key) => provider.GetRequiredKeyedService<MqttManager>(key).Current
            );
        }
    }
}
