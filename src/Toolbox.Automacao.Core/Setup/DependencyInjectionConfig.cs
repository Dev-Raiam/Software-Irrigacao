using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;
using Toolbox.Automacao.Core.Services.Automacao;
using Toolbox.Automacao.Core.Services.Cryptography;
using Toolbox.Automacao.Core.Services.Mqtt;
using static Toolbox.Automacao.Core.Services.Mqtt.IMqtt;

namespace Toolbox.Automacao.Core.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            //services.AddScoped<IConfiguracaoAutenticacao, ConfiguracaoAutenticacao>();

            services.AddTransient<IGerenciadorConfiguracao, GerenciadorConfiguracao>();

            services.AddTransient<IServicoAutomacao, ServicoAutomacao>();

            services.AddTransient<ISincronizacao, Sincronizacao>();

            services.AddTransient<IDadosSincronizacao, DadosSincronizacao>();

            services.AddTransient<ICryptography, Cryptography>();

            services.AddTransient<AuthenticationHandler>();

            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(
                @"Filename=Automacao.db;Connection=Shared"
            ));
            services.AddKeyedSingleton<MqttManager>(Mqtt.Local, (provider, key) => 
            {
                var config = provider.GetRequiredKeyedService<IOptions<Configuration>>(key).Value;
                return new MqttManager(config); 
            });

            services.AddKeyedSingleton<MqttManager>(Mqtt.Remoto, (provider, key) => 
            {
                //var config = provider.GetRequiredKeyedService<IOptions<Configuration>>(key).Value;
                var config = new Configuration
                {
                    Host = "broker.freemqtt.com",
                    Port = 1883,
                    ClientId = Guid.NewGuid().ToString(),
                    Username = "freemqtt",
                    Password = "public",
                };
                return new MqttManager(config); 
            });

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
