using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;

namespace Toolbox.Automacao.Core.Setup
{
    public static class ModuloConfig
    {
        public static void AddModuloCore(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionString) 
        {
            services.AddSingleton<ApiConfiguracao>();
            services.AddSingleton<MqttConfiguracao>();

            services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguracao"));
            services.Configure<MqttConfiguracao>(configuration.GetSection("MqttConfiguracao"));


            var keysPath = configuration["DataProtection:KeysPath"] ?? Path.Combine(AppContext.BaseDirectory, "Keys");

            services
                .AddDataProtection()
                .SetApplicationName("Automacao")
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            services.AddHttpClient("Toolbox", (provider,http )=> 
            {
                var apiConfiguracao = provider.GetRequiredService<IOptions<ApiConfiguracao>>().Value;

                var baseUrl = apiConfiguracao?.BaseUrl ?? "https://localhost";
                var timeout = apiConfiguracao?.TimeoutSeconds ?? 30;

                http.BaseAddress = new Uri(baseUrl);
                http.Timeout = TimeSpan.FromSeconds(timeout);

            }).AddHttpMessageHandler<AutenticacaoHandler>();

            services.AddDbContext<SincronizacaoDbContext>(options =>
                options.UseSqlite(
                    connectionString
                )
            );

            services.RegisterServices();
        }
    }
}
