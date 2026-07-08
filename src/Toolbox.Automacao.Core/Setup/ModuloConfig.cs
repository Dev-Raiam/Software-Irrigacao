using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Services;

namespace Toolbox.Automacao.Core.Setup
{
    public static class ModuloConfig
    {
        public static void AddModuloCore(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionString
        )
        {
            services.AddSingleton<ApiConfiguracao>();
            services.AddSingleton<MqttConfiguracao>();

            services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguracao"));
            services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));

            var keysPath =
                configuration["DataProtection:KeysPath"]
                ?? Path.Combine(AppContext.BaseDirectory, "Keys");

            services
                .AddDataProtection()
                .SetApplicationName("Automacao")
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            services.AddHttpClient<IServicoAutenticacao,ServicoAutenticacao>(
                (provider, http) => 
                {
                    var configuracao = provider.GetRequiredService<IOptions<ApiConfiguracao>>().Value;

                    if (string.IsNullOrWhiteSpace(configuracao?.BaseUrl))
                    {
                        throw new InvalidOperationException(
                            "A configuração 'ApiConfiguracao:BaseUrl' não foi encontrada ou está vazia no appsettings.json!");
                    }
                    
                    http.BaseAddress = new Uri(configuracao.BaseUrl);

                }).AddStandardResilienceHandler();

            services
                .AddHttpClient(HttpClientNames.Automacao, 
                (provider, http) => 
                {
                    var configuracao = provider.GetRequiredService<IOptions<ApiConfiguracao>>().Value;

                    if (string.IsNullOrWhiteSpace(configuracao?.BaseUrl))
                    {
                        throw new InvalidOperationException(
                            "A configuração 'ApiConfiguracao:BaseUrl' não foi encontrada ou está vazia no appsettings.json!");
                    }

                    http.BaseAddress = new Uri(configuracao.BaseUrl); ;
                })
                .AddHttpMessageHandler<AutenticacaoHandler>()
                .AddStandardResilienceHandler();

            services.AddDbContext<SincronizacaoDbContext>(options =>
                options.UseSqlite(connectionString)
            );

            services.RegisterServices();
        }
    }
}
