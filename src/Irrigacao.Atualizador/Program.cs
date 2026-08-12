using Irrigacao.Atualizador;
using Toolbox.Industrial.Core.Setup;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddHostedService<Atualizador>();

const string ConnectionString =
        $"Filename=irrigacao.db;Collation=pt-BR/IgnoreCase,IgnoreNonSpace;Connection=Shared";

builder.Services
    .AddIndustrialCoreAtualizador()
    .AddLiteDbEntityStore(ConnectionString);

var host = builder.Build();
host.Run();
