using Irrigacao.Atualizador;
using Toolbox.Industrial.Core.Setup;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton(
    new UpdateInstallationConfig(
        BinaryName: "irrigacao",
        ServiceName: "irrigacao",
        UpdateDirectory: Path.Combine(Path.GetTempPath(), "irrigacao-update"),
        BackupPath: "/opt/edge-plc-backup"
    )
);
builder.Services.AddHostedService<Updater>();

await Application.UpdateRun(
    builder,
    integracaoId: Guid.Parse("2eb57304-1df3-4883-8f81-29b3e9426f6c"),
    connectionString: $"Filename=irrigacao.db;Collation=pt-BR/IgnoreCase,IgnoreNonSpace;Connection=Shared"
);
