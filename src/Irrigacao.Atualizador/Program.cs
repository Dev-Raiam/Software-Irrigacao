using System.Reflection;
using Irrigacao.Atualizador;
using Serilog;
using Toolbox.Industrial.Core.Setup;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

if (args.Length > 0 && (args[0] == "--version" || args[0] == "-v"))
{
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    Console.WriteLine(version);
    return;
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
    )
    .CreateBootstrapLogger();

try
{
    Log.Information($"Inicializando Aplicação");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddHttpClient();

    builder.Services.AddSingleton(
        new UpdateInstallationConfig(
            BinaryName: "irrigacao",
            ServiceName: "irrigacao",
            BinaryDirectory: "/opt/edge-plc",
            UpdateDirectory: "/var/tmp/edge-plc",
            BackupPath: "/var/backups/edge-plc",
            Url: "/automacao/v1/integracoes/2eb57304-1df3-4883-8f81-29b3e9426f6c/atualizacao-disponivel"
        )
    );

    builder.Services.AddHostedService<Updater>();

    await Application.RunAsync(
        builder,
        integracaoId: Guid.Parse("2eb57304-1df3-4883-8f81-29b3e9426f6c"),
        connectionString: $"Filename=irrigacao.db;Collation=pt-BR/IgnoreCase,IgnoreNonSpace;Connection=Shared"
    );
}
catch (Exception ex)
{
    Log.Fatal("A Aplica��o falhou ao iniciar. {ex}", ex);
}
finally
{
    Log.CloseAndFlush();
}
