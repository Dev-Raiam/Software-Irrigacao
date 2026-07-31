using System.Text;
using Newtonsoft.Json;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;

namespace Toolbox.Industrial.Core.Data;

public class PythonSettings
{
    public virtual int Version { get; } = 1;

    public virtual DateTime GeneratedAt { get; } = DateTime.UtcNow;

    public virtual required MqttConfiguration Mqtt { get; init; }
}

public interface IPythonSettingsExporter
{
    bool Exported { get; }
    Task ExportAsync(CancellationToken cancellationToken = default);
}

public class PythonSettingsExporter : IPythonSettingsExporter
{
    private readonly string _file;
    private bool _exported = false;
    private readonly IEntityStore _store;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public PythonSettingsExporter(IEntityStore store)
    {
        _file = "py_settings.json";
        _store = store;
    }

    public virtual bool Exported => _exported;

    public virtual async Task ExportAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        _exported = false;
        try
        {
            var settings = new PythonSettings
            {
                Mqtt = (MqttConfiguration)
                    (
                        await _store.FirstOrDefaultAsync<Configuracao>(x =>
                            x.Id == Entity.Keys.Mqtt.LocalPython
                        )
                    ).Valor,
            };

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            var temp = _file + ".tmp";

            await File.WriteAllTextAsync(temp, json, new UTF8Encoding(false), cancellationToken);

            File.Move(temp, _file, true);

            _exported = true;
        }
        finally
        {
            _lock.Release();
        }
    }
}
