using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;

namespace Toolbox.Automacao.Core.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services) 
        {
            services.AddScoped<IServicoAutomacao, ServicoAutomacao>();
            services.AddScoped<ISincronizarControladores, SincronizarControladores>();
            services.AddScoped<IProvedorDataSincronizacao, ProvedorDataSincronizacao>();

            services.AddSingleton<ICriptografia, Criptografia>();
            services.AddSingleton<Token>();

            services.AddTransient<AutenticacaoHandler>();

            services.AddHttpClient<IServicoAutenticacao, ServicoAutenticacao>();
        }
    }
}
