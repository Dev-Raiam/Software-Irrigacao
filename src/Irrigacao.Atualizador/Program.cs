using Irrigacao.Atualizador;
using Toolbox.Industrial.Core.Setup;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddHostedService<Atualizador>();

Application.UpdateRun(builder, $"Filename=irrigacao.db;Collation=pt-BR/IgnoreCase,IgnoreNonSpace;Connection=Shared");
