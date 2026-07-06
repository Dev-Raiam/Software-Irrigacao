using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services;
using Toolbox.Automacao.Core.Services.Automacao.Autenticacao;

namespace Toolbox.Automacao.Core.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IConfiguracaoAutenticacao, ConfiguracaoAutenticacao>();
            services.AddScoped<IServicoAutomacao, ServicoAutomacao>();
            services.AddScoped<ISincronizarControladores, SincronizarControladores>();
            services.AddScoped<IProvedorDataSincronizacao, ProvedorDataSincronizacao>();

            services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase(
                @"Filename=Automacao.db;Connection=Shared"
            ));

            services.AddSingleton<ICriptografia, Criptografia>();
            services.AddSingleton<Token>();
            services.AddTransient<AutenticacaoHandler>();
            services.AddHttpClient<IServicoAutenticacao, ServicoAutenticacao>();
        }
    }
}
