using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Toolbox.Industrial.Core.Services.Api;

namespace Toolbox.Industrial.Core.Setup
{
    public static class ModuloConfig
    {
        public static void AddModuloCore(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionString
        )
        {
            services.AddSingleton<Configuration>();
            //services.AddSingleton<MqttConfiguracao>();

            services.Configure<Configuration>(configuration.GetSection("ApiConfiguracao"));
            //services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));

            var keysPath =
                configuration["DataProtection:KeysPath"]
                ?? Path.Combine(AppContext.BaseDirectory, "Keys");

            services
                .AddDataProtection()
                .SetApplicationName("Automacao")
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            services
                .AddHttpClient<IApiClient, ApiClient>(
                    (provider, http) =>
                    {
                        var configuracao = provider
                            .GetRequiredService<IOptions<Configuration>>()
                            .Value;

                        if (string.IsNullOrWhiteSpace(configuracao?.BaseUrl))
                        {
                            throw new InvalidOperationException(
                                "A configuração 'ApiConfiguracao:BaseUrl' não foi encontrada ou está vazia no appsettings.json!"
                            );
                        }

                        http.BaseAddress = new Uri(configuracao.BaseUrl);
                    }
                )
                .AddStandardResilienceHandler();

            services
                .AddHttpClient(
                    HttpClientNames.Automacao,
                    (provider, http) =>
                    {
                        var configuracao = provider
                            .GetRequiredService<IOptions<Configuration>>()
                            .Value;

                        if (string.IsNullOrWhiteSpace(configuracao?.BaseUrl))
                        {
                            throw new InvalidOperationException(
                                "A configuração 'ApiConfiguracao:BaseUrl' não foi encontrada ou está vazia no appsettings.json!"
                            );
                        }

                        http.BaseAddress = new Uri(configuracao.BaseUrl);
                    }
                )
                .AddHttpMessageHandler<AuthGuard>()
                .AddStandardResilienceHandler();

            services.RegisterServices();
        }
    }
}
