using Irrigacao.Atualizador;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddHostedService<Atualizador>();

var host = builder.Build();
host.Run();
