using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using NModbus;
using NModbus.Device;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;
using Toolbox.Automacao.Core.Services.Modbus;

namespace Toolbox.Automacao.Core.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            //services.AddScoped<IConfiguracaoAutenticacao, ConfiguracaoAutenticacao>();

            services.AddTransient<IGerenciadorConfiguracao, GerenciadorConfiguracao>();

            services.AddTransient<IServicoAutomacao, ServicoAutomacao>();

            services.AddTransient<ISincronizarControladores, SincronizarControladores>();

            services.AddTransient<IProvedorDataSincronizacao, ProvedorDataSincronizacao>();

            services.AddTransient<ICriptografia, Criptografia>();

            services.AddTransient<AutenticacaoHandler>();

            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(
                @"Filename=Automacao.db;Connection=Shared"
            ));

            services.AddSingleton<Token>();
        }
    }
}
